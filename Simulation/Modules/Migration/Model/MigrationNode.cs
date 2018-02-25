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
    public class MigrationNode : IStateNode
    {
        public class MigrationRecord
        {
            public VM Target { get; set; }
            public Server Reciever { get; set; }

            public MigrationRecord(VM target, Server reciever)
            {
                Target = target;
                Reciever = reciever;
            }
            // TODO: do I need time?
        }

        public MigrationRecord[] Changes { get; private set; }
        private MigrationRootNode _root;
        private int _turnOnCount;

        public bool IsValid { get; private set; }

        public float Value => _value.Value;
        private Lazy<float> _value;

        private float CulculateValue()
        {
            float val = 0f;

            val -= GlobalConstants.TURN_ON_SHTRAF;

            val += GetTargetServerResourcesChange(_root.Depth).EvaluateVolume();

            // TODO: add value of average resources usage on recievers

            return val;
        }

        private Resources GetTargetServerResourcesChange(byte depth)
        {
            Resources r = new Resources();
            Changes.ForEach((changes) => r += changes.Target.PrognosedResources[depth]);

            return r;
        }

        private Resources GetRecieverResourcesChange(Server server, byte depth)
        {
            Resources r = new Resources();
            Changes
                .Where((change) => change.Reciever.Id == server.Id)
                .ForEach((changes) => r += changes.Target.PrognosedResources[depth]);

            return r;
        }

        public IEnumerable<IStateNode> GetChilds()
        {
            return _root.TargetServer.RunningVMs
                .Where(vm => Changes.All(record => record.Target.Id != vm.Id))
                .OrderByDescending((vm) => vm.Resources.EvaluateVolume())
                .Take(GlobalConstants.VM_PER_SERVER)
                .SelectMany((vm) => CreateChildNodes(vm));
        }

        private IEnumerable<MigrationNode> CreateChildNodes(VM vm)
        {
            var nodes = _root.Recievers
                .Where(server => CanServerRunVM(server, vm)) // !
                .OrderByDescending(server => Evaluator.Evaluate(
                    server.Resources - server.PrognosedUsedResources[_root.Depth] - GetRecieverResourcesChange(server, _root.Depth)))
                .Select(server => new MigrationNode(this, vm, server)).ToList();
            var remainigCount = GlobalConstants.MIN_CHILD_NODES_PER_VM - nodes.Count;
            if (remainigCount > 0)
            {
                var toTurnOn = _root.Reservation
                    .Where(server => CanServerRunVM(server, vm))
                    .OrderByDescending(server => Evaluator.Evaluate(
                        server.Resources - server.PrognosedUsedResources[_root.Depth] - GetRecieverResourcesChange(server, _root.Depth)))
                    .Take(remainigCount)
                    .Select(server => new MigrationNode(this, vm, server, true));

                nodes.AddRange(toTurnOn);

                remainigCount -= toTurnOn.Count();
            }
            if (remainigCount > 0)
            {
                // TODO: consider swaps... or not?
            }
            return nodes;
        }

        private bool CanServerRunVM(Server server, VM vm)
        {
            bool result = true;

            for (byte i = 0; i < _root.Depth; i++)
            {
                result = !Evaluator.IsOverloaded(
                    server.PrognosedUsedResources[i] + GetRecieverResourcesChange(server, i),
                    server, i);
                if (!result)
                    return result;
            }
            return result;
        }

        public MigrationNode(MigrationRootNode root, VM target, Server reciever, bool turnOnNew = false)
        {
            _value = new Lazy<float>(CulculateValue);
            Changes = new MigrationRecord[1] { new MigrationRecord(target, reciever) };
            _root = root;
            for (byte i = 0; i < _root.Depth; i++)
            {
                IsValid = !Evaluator.IsOverloaded(
                    _root.TargetServer.PrognosedUsedResources[i] - target.PrognosedResources[i],
                    _root.TargetServer, i);
                if (!IsValid)
                    break;
            }
            if (turnOnNew)
                _turnOnCount = 1;
        }

        public MigrationNode(MigrationNode parent, VM target, Server reciever, bool turnOnNew = false)
        {
            _value = new Lazy<float>(CulculateValue);
            Changes = parent.Changes.PushToEnd(new MigrationRecord(target, reciever));
            _root = parent._root;
            _turnOnCount = parent._turnOnCount;
            for (byte i = 0; i < _root.Depth; i++)  // TODO: is cycle neccessary ?
            {
                IsValid = !Evaluator.IsOverloaded(
                    _root.TargetServer.PrognosedUsedResources[i] - GetTargetServerResourcesChange(i),
                    _root.TargetServer, i);
                if (!IsValid)
                    break;
            }
            if (turnOnNew)
                _turnOnCount += 1;
        }
    }
}
