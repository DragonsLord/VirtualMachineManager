using Simulation.Models;
using Simulation.Modules.Diagnostic;
using Simulation.Modules.Migration.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Simulation.Modules.Migration.Model
{
    public abstract class MigrationNode : IStateNode
    {
        public class MigrationRecord
        {
            public VM Target { get; set; }
            public Server Reciever { get; set; }
            public Resources MigrationRequirment { get; private set; }

            public MigrationRecord(VM target, Server reciever, Server sender)
            {
                Target = target;
                Reciever = reciever;
                MigrationRequirment = Evaluator.GetMigrationResourceRequirments(reciever, sender);
            }
        }

        public MigrationRecord[] Changes { get; private set; }
        protected MigrationRootNode _root;
        protected int _turnOnCount;

        public bool IsValid { get; private set; }

        public float Value => _value;
        private float _value;

        protected Resources GetTargetServerResourcesChange(byte depth)
        {
            Resources r = new Resources();
            Changes.ForEach((changes) => 
                r += (changes.MigrationRequirment - changes.Target.Resources));
            return r;
        }

        public Resources GetRecieverUsedResources(Server server, byte depth)
        {
            Resources r = new Resources();
            Changes
                .Where((change) => change.Reciever.Id == server.Id)
                .ForEach((changes) =>
                    r += (changes.Target.Resources + changes.MigrationRequirment));

            return r + server.PrognosedUsedResources[depth];
        }

        public IEnumerable<IStateNode> GetChilds()
        {
            return _root.TargetServer.RunningVMs
                .Where(vm => !vm.IsMigrating)
                .Where(vm => Changes.All(record => record.Target.Id != vm.Id))
                .OrderByDescending((vm) => vm.Resources.EvaluateVolume())
                .Take(GlobalConstants.VM_PER_SERVER)
                .SelectMany((vm) => CreateChildNodes(vm));
        }

        private IEnumerable<IStateNode> CreateChildNodes(VM vm)
        {
            var nodes = _root.Recievers
                .Where(server => CanServerRunVM(server, vm)) // !
                .OrderByDescending(server => Evaluator.Evaluate(
                    server.Resources - GetRecieverUsedResources(server, _root.Depth)))
                .Select(server => CreateNode(this, vm, server)).ToList();
            var remainigCount = GlobalConstants.MIN_CHILD_NODES_PER_VM - nodes.Count;
            if (remainigCount > 0)
            {
                var toTurnOn = _root.Reservation
                    .Where(server => CanServerRunVM(server, vm))
                    .OrderByDescending(server => Evaluator.Evaluate(
                        server.Resources - GetRecieverUsedResources(server, _root.Depth)))
                    .Take(remainigCount)
                    .Select(server => CreateNode(this, vm, server, true));

                nodes.AddRange(toTurnOn);

                remainigCount -= toTurnOn.Count();
            }
            return nodes;
        }

        private bool CanServerRunVM(Server server, VM vm)
        {
            for (byte i = 0; i < _root.Depth; i++)
            {
                if (Evaluator.IsOverloaded(
                    GetRecieverUsedResources(server, i),
                    server))
                {
                    return false;
                }
            }
            return true;
        }

        public MigrationNode(MigrationRootNode root, VM target, Server reciever, bool turnOnNew = false)
        {
            _root = root;
            Changes = new MigrationRecord[1] { new MigrationRecord(target, reciever, _root.TargetServer) };
            IsValid = CalculateValidity();
            if (turnOnNew)
                _turnOnCount = 1;
            _value = CalculateValue(root, turnOnNew);
        }

        public MigrationNode(MigrationNode parent, VM target, Server reciever, bool turnOnNew = false)
        {
            _root = parent._root;
            Changes = parent.Changes.PushToEnd(new MigrationRecord(target, reciever, _root.TargetServer));
            _turnOnCount = parent._turnOnCount;
            IsValid = CalculateValidity();
            if (turnOnNew)
                _turnOnCount += 1;
            _value = CalculateValue(parent, turnOnNew);
        }

        protected abstract bool CalculateValidity();

        protected abstract float CalculateValue(IStateNode previous, bool newTurnOn);

        protected abstract IStateNode CreateNode(MigrationNode parent, VM target, Server reciever, bool turnOnNew = false);
    }
}
