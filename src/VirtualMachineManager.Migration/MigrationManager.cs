using System;
using System.Collections.Generic;
using System.Linq;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.EvaluationExtensions;
using VirtualMachineManager.Migration.Algorythm;
using VirtualMachineManager.Migration.Model;
using VirtualMachineManager.Services;

namespace VirtualMachineManager.Migration
{
    public class MigrationManager
    {
        private readonly BeamSearchAlgorythm _searchEngine;
        private readonly MigrationParams _params;
        private readonly IServerManager _serverManager;

        public MigrationManager(MigrationParams config, IServerManager serverManager)
        {
            _params = config;
            MigrationParams.Current = _params;
            _serverManager = serverManager;
            _searchEngine = new BeamSearchAlgorythm(config.BeamLength);
        }

        public MigrationPlan MigrateFromOverloaded(IEnumerable<Server> targets, IEnumerable<Server> recievers)
        {
            var migrationPlan = new MigrationPlan();
            var targetsCopies = recievers.Select(Copy)
                .OrderBy(s => s.ResourcesCapacity.GetValue())
                .ThenBy(s => s.RunningVMs.Count)
                .ThenBy(s => s.UsedResources.GetValue() / s.RunningVMs.Count)
                .ToList();

            var priorityRecievers = targetsCopies.Where(server => server.TurnedOn);
            var reserve = targetsCopies.Where(server => !server.TurnedOn);
            foreach (var server in targets)
            {
                var resultNode = _searchEngine.Run(
                    new MigrationRootNode(
                        server,
                        priorityRecievers.ToList(),
                        reserve.ToList(),
                        GetInitialValue(priorityRecievers, EvaluateForOverloading),
                        OverloadedMigrationNode.FromRootNode)
                    ) as MigrationNode;
                if (resultNode == null)
                    break;
                migrationPlan.Add(resultNode, server);
                foreach (var change in resultNode.Changes)
                {
                    var srv = targetsCopies.Find(s => s.Id == change.Reciever.Id);
                    _serverManager.RunVM(srv, change.Target);
                    srv.UsedResources += server.GetMigrationResourceRequirments(srv);
                }
            }
            return migrationPlan;
        }

        public MigrationPlan ReleaseLowloadedMachines(IEnumerable<Server> targets, IEnumerable<Server> recieversCandidates) // TODO: Migration Res consider too late
        {
            var migrationPlan = new MigrationPlan();
            var targetsCopies = recieversCandidates.Select(Copy)
                .OrderBy(s => s.ResourcesCapacity.GetValue())
                .ThenBy(s => s.RunningVMs.Count)
                .ThenBy(s => s.UsedResources.GetValue() / s.RunningVMs.Count)
                .ToList();

            var recievers = targetsCopies.Where(server => server.TurnedOn);
            foreach (var server in targets)
            {
                var resultNode = _searchEngine.Run(
                    new MigrationRootNode(
                        server,
                        recievers.ToList(),
                        new List<Server>(), // empty reserve because we are decreasing amount of working servers
                        GetInitialValue(recievers, EvaluateForReleasing),
                        LowloadedMigrationNode.FromRootNode)
                    ) as MigrationNode;
                if (resultNode == null || !resultNode.IsValid)  // apply migration only if all VM is going to migrate
                    break;
                migrationPlan.Add(resultNode, server);
                foreach (var change in resultNode.Changes)
                {
                    var reciever = targetsCopies.Find(s => s.Id == change.Reciever.Id);
                    _serverManager.RunVM(reciever, change.Target);
                    reciever.UsedResources += server.GetMigrationResourceRequirments(reciever);
                }
            }
            return migrationPlan;
        }

        public List<MigrationTask> ApplyMigrations(MigrationPlan migrationPlan)
        {
            var migrationTasks = new List<MigrationTask>(migrationPlan.Count);

            foreach (var migration in migrationPlan)
            {
                migrationTasks.Add(new MigrationTask(
                        migration.Target,
                        _serverManager.Get(migration.SourceId),
                        _serverManager.Get(migration.RecieverId)));
            }

            return migrationTasks;
        }

        private Server Copy(Server server) =>
            new Server()
            {
                Id = server.Id,
                ResourcesCapacity = server.ResourcesCapacity,
                TurnedOn = server.TurnedOn,
                UsedResources = server.UsedResources,
                RunningVMs = server.RunningVMs.ToList()
            };

        private float GetInitialValue(IEnumerable<Server> recievers, Func<Server, float> evaluator) =>
            recievers.Any() ? recievers.Average(evaluator) : 0;

        private float EvaluateForOverloading(Server server)
        {
            var usedResources = server.UsedResources;
            var toFreeCap = new Resources()
            {
                CPU = server.ResourcesCapacity.CPU * _params.LowLevel.CPU,
                Memmory = server.ResourcesCapacity.Memmory * _params.LowLevel.Memmory,
                Network = server.ResourcesCapacity.Network * _params.LowLevel.Network,
                IOPS = server.ResourcesCapacity.IOPS * _params.LowLevel.IOPS
            };
            if (usedResources < toFreeCap)
            {
                return (toFreeCap - usedResources).GetValue();
            }
            else
            {
                var desiredLevel = new Resources()
                {
                    CPU = server.ResourcesCapacity.CPU * _params.DesiredLevel.CPU,
                    Memmory = server.ResourcesCapacity.Memmory * _params.DesiredLevel.Memmory,
                    Network = server.ResourcesCapacity.Network * _params.DesiredLevel.Network,
                    IOPS = server.ResourcesCapacity.IOPS * _params.DesiredLevel.IOPS
                };
                // "-" is used to transform it to maximization task
                return -Math.Abs((desiredLevel - usedResources).GetValue());
            }
        }

        public float EvaluateForReleasing(Server server)
        {
            var usedResources = server.UsedResources;
            var desiredLevel = new Resources()
            {
                CPU = server.ResourcesCapacity.CPU * _params.DesiredLevel.CPU,
                Memmory = server.ResourcesCapacity.Memmory * _params.DesiredLevel.Memmory,
                Network = server.ResourcesCapacity.Network * _params.DesiredLevel.Network,
                IOPS = server.ResourcesCapacity.IOPS * _params.DesiredLevel.IOPS
            };
            // "-" is used to transform it to maximization task
            return -Math.Abs((desiredLevel - usedResources).GetValue());
        }
    }
}
