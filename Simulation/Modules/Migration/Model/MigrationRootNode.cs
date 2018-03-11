using Simulation.Models;
using Simulation.Modules.Migration.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Simulation.Modules.Migration.Model
{
    public class MigrationRootNode : IStateNode
    {
        public byte Depth { get; }
        public Server TargetServer { get; }
        public IEnumerable<Server> Recievers { get; }
        public IEnumerable<Server> Reservation { get; }

        public float Value => 0;

        public bool IsValid => false;

        public IEnumerable<IStateNode> GetChilds()
        {
            return TargetServer.RunningVMs
                .Where(vm => !vm.IsMigrating)
                .OrderByDescending((vm) => vm.Resources.EvaluateVolume())
                .Take(GlobalConstants.VM_PER_SERVER)
                .SelectMany((vm) => CreateChildNodes(vm));
        }

        private IEnumerable<MigrationNode> CreateChildNodes(VM vm)
        {
            var nodes = Recievers
                .Where(server => server.CanRunVM(vm, Depth))
                .OrderByDescending(server => (server.Resources - server.PrognosedUsedResources[Depth]).EvaluateVolume())
                .Select(server => new MigrationNode(this, vm, server)).ToList();
            var remainigCount = GlobalConstants.MIN_CHILD_NODES_PER_VM - nodes.Count;
            if (remainigCount > 0)
            {
                var toTurnOn = Reservation
                    .Where(server => server.CanRunVM(vm, Depth))
                    .OrderByDescending(server => (server.Resources - server.PrognosedUsedResources[Depth]).EvaluateVolume())
                    .Take(remainigCount)
                    .Select(server => new MigrationNode(this, vm, server, true));

                nodes.AddRange(toTurnOn);

                remainigCount -= toTurnOn.Count();
            }
            return nodes;
        }

        public MigrationRootNode(Server targetServer, IEnumerable<Server> recievers, IEnumerable<Server> reservation, byte depth)
        {
            TargetServer = targetServer;
            Recievers = recievers;
            Reservation = reservation;
            Depth = depth;
        }
    }
}
