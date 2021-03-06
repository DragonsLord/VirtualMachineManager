﻿using System;
using System.Collections.Generic;
using System.Linq;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.EvaluationExtensions;
using VirtualMachineManager.Migration.Algorythm;
using VirtualMachineManager.Migration.Model;

namespace VirtualMachineManager.Migration
{
    public class MigrationManager
    {
        private readonly BeamSearchAlgorythm _searchEngine;
        private readonly MigrationParams _params;

        public MigrationManager(MigrationParams config)
        {
            _params = config;
            MigrationParams.Current = _params;
            _searchEngine = new BeamSearchAlgorythm(config.BeamLength);
        }

        public MigrationPlan MigrateFromOverloaded(IEnumerable<Server> targets, IEnumerable<Server> recievers)
        {
            var migrationPlan = new MigrationPlan();
            var targetsCopies = recievers.Select(s => s.Copy())
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
                // TODO: reconsider
                foreach (var change in resultNode.Changes)
                {
                    var srv = targetsCopies.Find(s => s.Id == change.Reciever.Id);
                    srv.AsignVM(change.Target);
                    srv.UsedResources += server.GetMigrationResourceRequirments(srv);
                }
            }
            return migrationPlan;
        }

        public MigrationPlan ReleaseLowloadedMachines(IEnumerable<Server> targets, IEnumerable<Server> recieversCandidates) // TODO: Migration Res consider too late
        {
            var migrationPlan = new MigrationPlan();
            var targetsCopies = recieversCandidates.Select(s => s.Copy())
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
                    reciever.AsignVM(change.Target);
                    reciever.UsedResources += server.GetMigrationResourceRequirments(reciever);
                }
            }
            return migrationPlan;
        }

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

        private float EvaluateForReleasing(Server server)
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
