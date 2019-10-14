using System.Collections.Generic;
using System.Linq;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.EvaluationExtensions;
using VirtualMachineManager.Migration.Algorythm.Interfaces;

namespace VirtualMachineManager.Migration.Model
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
                MigrationRequirment = sender.GetMigrationResourceRequirments(reciever);
            }
        }

        public MigrationRecord[] Changes { get; private set; }
        protected MigrationRootNode _root;
        protected int _turnOnCount;

        public bool IsValid { get; private set; }

        public float Value => _value;
        private float _value;

        protected Resources GetTargetServerResourcesChange()
        {
            return Changes.Aggregate(
                new Resources(),
                (r, changes) => r + (changes.MigrationRequirment - changes.Target.Resources));
        }

        public Resources GetRecieverUsedResources(Server server)
        {
            var res = Changes
                .Where((change) => change.Reciever.Id == server.Id)
                .Aggregate(
                    new Resources(),
                    (r, changes) => r + (changes.Target.Resources + changes.MigrationRequirment));

            return res + server.UsedResources;
        }

        public IEnumerable<IStateNode> GetChilds()
        {
            return _root.TargetServer.RunningVMs
                .Where(vm => !vm.IsMigrating)
                .Where(vm => Changes.All(record => record.Target.Id != vm.Id))
                .OrderByDescending((vm) => vm.Resources.GetValue())
                .Take(MigrationParams.Current.MaxMigrateCandidatesPerStep) // GlobalConstants.VM_PER_SERVER
                .SelectMany((vm) => CreateChildNodes(vm));
        }

        private IEnumerable<IStateNode> CreateChildNodes(VM vm)
        {
            var nodes = _root.Recievers
                .Where(server => CanServerRunVM(server, vm)) // !
                .OrderByDescending(server => GetAviableResources(server).GetValue())
                .Select(server => CreateNode(this, vm, server)).ToList();
            var remainigCount = MigrationParams.Current.MinChildNodesPerVM - nodes.Count; // MIN_CHILD_NODES_PER_VM
            if (remainigCount > 0)
            {
                var toTurnOn = _root.Reservation
                    .Where(server => CanServerRunVM(server, vm))
                    .OrderByDescending(server => GetAviableResources(server).GetValue())
                    .Take(remainigCount)
                    .Select(server => CreateNode(this, vm, server, true));

                nodes.AddRange(toTurnOn);

                remainigCount -= toTurnOn.Count();
            }
            return nodes;
        }

        private bool CanServerRunVM(Server server, VM vm) =>
            !server.IsOverloaded(GetRecieverUsedResources(server) + vm.Resources); // !! vm part was missing

        private Resources GetAviableResources(Server server) =>
            server.ResourcesCapacity - GetRecieverUsedResources(server);

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
            Changes = parent.Changes.Append(new MigrationRecord(target, reciever, _root.TargetServer)).ToArray();
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
