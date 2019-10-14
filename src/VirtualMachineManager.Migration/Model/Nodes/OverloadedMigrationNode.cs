using System.Linq;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.EvaluationExtensions;
using VirtualMachineManager.Migration.Algorythm.Interfaces;

namespace VirtualMachineManager.Migration.Model
{
    public class OverloadedMigrationNode : MigrationNode
    {
        public OverloadedMigrationNode(MigrationRootNode root, VM target, Server reciever, bool turnOnNew) 
            : base(root, target, reciever, turnOnNew)
        {
        }

        public OverloadedMigrationNode(MigrationNode parent, VM target, Server reciever, bool turnOnNew = false)
            : base(parent, target, reciever, turnOnNew)
        {
        }

        protected override bool CalculateValidity() =>
            !_root.TargetServer.IsOverloaded(
                _root.TargetServer.UsedResources + GetTargetServerResourcesChange()
            );

        protected override float CalculateValue(IStateNode previous, bool newTurnOn)
        {
            float val = previous.Value;
            
            val += GetTargetServerResourcesChange().GetValue();

            var change = Changes.Last();
            var newServerResources = GetRecieverUsedResources(change.Reciever);

            if (newTurnOn) {
                val -= MigrationParams.Current.ServerTurnOnPenalty;
                val += CalculateDiffForNewMachineCase(previous.Value, newServerResources);
            } else {
                val += CalculateDiffForUpdatedMachineCase(change.Target.Resources);
            }

            return val;
        }

        private float CalculateDiffForUpdatedMachineCase(Resources serverResourcesChange)
        {
            var totalServers = _root.Recievers.Count() + _turnOnCount;
            return serverResourcesChange.GetValue() / totalServers;
        }

        private float CalculateDiffForNewMachineCase(float oldValue, Resources newServerValue)
        {
            var totalServers = _root.Recievers.Count() + _turnOnCount;
            return (newServerValue.GetValue() - oldValue) / totalServers;
        }

        protected override IStateNode CreateNode(MigrationNode parent, VM target, Server reciever, bool turnOnNew = false)
        {
            return new OverloadedMigrationNode(parent, target, reciever, turnOnNew);
        }

        public static MigrationNode FromRootNode(MigrationRootNode root, VM target, Server reciever, bool turnOnNew)
        {
            return new OverloadedMigrationNode(root, target, reciever, turnOnNew);
        }
    }
}
