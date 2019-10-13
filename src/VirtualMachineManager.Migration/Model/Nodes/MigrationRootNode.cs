﻿using System;
using System.Collections.Generic;
using System.Linq;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.Migration.Algorythm.Interfaces;

namespace VirtualMachineManager.Migration.Model
{
    public class MigrationRootNode : IStateNode
    {
        private Func<MigrationRootNode, VM, Server, bool, MigrationNode> CreateChildNode;
        public byte Depth { get; }
        public Server TargetServer { get; }
        public IEnumerable<Server> Recievers { get; }
        public IEnumerable<Server> Reservation { get; }

        public float Value { get; }

        public bool IsValid => false;

        public IEnumerable<IStateNode> GetChilds()
        {
            return TargetServer.RunningVMs
                .Where(vm => !vm.IsMigrating)
                .OrderByDescending((vm) => vm.Resources.EvaluateVolume())
                .Take(MigrationParams.Current.MaxMigrateCandidatesPerStep) // VM_PER_SERVER
                .SelectMany((vm) => CreateChildNodes(vm));
        }

        private IEnumerable<MigrationNode> CreateChildNodes(VM vm)
        {
            var nodes = Recievers
                // .Where(server => server.CanRunVM(vm, Depth))
                // .OrderByDescending(server => (server.ResourcesCapacity - server.UsedResources).EvaluateVolume())
                .Select(server => CreateChildNode(this, vm, server, false)).ToList();
            var remainigCount = MigrationParams.Current.MinChildNodesPerVM - nodes.Count; // MIN_CHILD_NODES_PER_VM
            if (remainigCount > 0)
            {
                var toTurnOn = Reservation
                    .Where(server => server.CanRunVM(vm, Depth))
                    .OrderByDescending(server => (server.Resources - server.PrognosedUsedResources[Depth]).EvaluateVolume())
                    .Take(remainigCount)
                    .Select(server => CreateChildNode(this, vm, server, true));

                nodes.AddRange(toTurnOn);

                remainigCount -= toTurnOn.Count();
            }
            return nodes;
        }

        public MigrationRootNode(
            Server targetServer, 
            IEnumerable<Server> recievers, 
            IEnumerable<Server> reservation, 
            byte depth,
            float initialValue,
            Func<MigrationRootNode, VM, Server, bool, MigrationNode> nodeCreator)
        {
            TargetServer = targetServer;
            Recievers = recievers;
            Reservation = reservation;
            Depth = depth;
            CreateChildNode = nodeCreator;
            Value = initialValue;
        }
    }
}
