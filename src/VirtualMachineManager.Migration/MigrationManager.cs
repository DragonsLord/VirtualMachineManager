using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.EvaluationExtensions;
using VirtualMachineManager.Migration.Algorythm;
using VirtualMachineManager.Migration.Model;

namespace VirtualMachineManager.Migration
{
    public class MigrationManager
    {
        private BeamSearchAlgorythm _searchEngine;

        public MigrationManager(MigrationParams config)
        {
            MigrationParams.Current = config;
            _searchEngine = new BeamSearchAlgorythm(config.BeamLength);
        }

        public MigrationPlan MigrateFromOverloaded(IEnumerable<Server> targets, IEnumerable<Server> recievers)
        {
            var migrationPlan = new MigrationPlan();
            var copies = recievers
                //.Select(server => server.ShalowCopy())
                .OrderBy(s => s.ResourcesCapacity.GetValue())
                .ThenBy(s => s.RunningVMs.Count)
                .ThenBy(s => s.UsedResources.GetValue() / s.RunningVMs.Count)
                .ToList();

            var priorityRecievers = copies.Where(server => server.TurnedOn);
            var reserve = copies.Where(server => !server.TurnedOn);
            foreach (var server in targets)
            {
                var resultNode = _searchEngine.Run(
                    new MigrationRootNode(
                        server,
                        priorityRecievers.ToList(),
                        reserve.ToList(),
                        GetInitialValue(priorityRecievers, Evaluator.EvaluateForOverloading),
                        OverloadedMigrationNode.FromRootNode)
                    ) as MigrationNode;
                if (resultNode == null)
                    break;
                migrationPlan.Add(resultNode, server);
                foreach (var change in resultNode.Changes)
                {
                    var srv = copies.Find(s => s.Id == change.Reciever.Id);
                    srv.RunVM(change.Target);
                    srv.UsedResources += server.GetMigrationResourceRequirments(srv);
                }
            }
            return migrationPlan;
        }

        public MigrationPlan ReleaseLowloadedMachines(IEnumerable<Server> targets, IEnumerable<Server> recieversCandidates) // TODO: Migration Res consider too late
        {
            var migrationPlan = new MigrationPlan();
            var copies = recieversCandidates
                //.Select(server => server.ShalowCopy())
                .OrderBy(s => s.ResourcesCapacity.GetValue())
                .ThenBy(s => s.RunningVMs.Count)
                .ThenBy(s => s.UsedResources.GetValue() / s.RunningVMs.Count)
                .ToList();

            var recievers = copies.Where(server => server.TurnedOn);
            foreach (var server in targets)
            {
                var resultNode = _searchEngine.Run(
                    new MigrationRootNode(
                        server,
                        recievers.ToList(),
                        new List<Server>(), // empty reserve because we are decreasing amount of working servers
                        GetInitialValue(recievers, Evaluator.EvaluateForReleasing),
                        LowloadedMigrationNode.FromRootNode)
                    ) as MigrationNode;
                if (resultNode == null || !resultNode.IsValid)  // apply migration only if all VM is going to migrate
                    break;
                migrationPlan.Add(resultNode, server);
                foreach (var change in resultNode.Changes)
                {
                    var reciever = copies.Find(s => s.Id == change.Reciever.Id);
                    reciever.RunVM(change.Target);
                    reciever.UsedResources += Evaluator.GetMigrationResourceRequirments(reciever, server);

                }
            }
            return migrationPlan;
        }

        public List<MigrationTask> ApplayMigrations(MigrationPlan migrationPlan, ServerCollection servers)
        {
            var migrationTasks = new List<MigrationTask>(migrationPlan.Count);

            foreach (var migration in migrationPlan)
            {
                migrationTasks.Add(new MigrationTask(
                        migration.Target,
                        servers.Get(migration.SourceId),
                        servers.Get(migration.RecieverId)));
            }

            return migrationTasks;
        }

        private float GetInitialValue(IEnumerable<Server> recievers, Func<Server, float> evaluator) =>
            recievers.Any() ? recievers.Average(evaluator) : 0;
    }
}
