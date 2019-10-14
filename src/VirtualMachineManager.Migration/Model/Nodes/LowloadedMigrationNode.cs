using System.Linq;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.EvaluationExtensions;
using VirtualMachineManager.Migration.Algorythm.Interfaces;

namespace VirtualMachineManager.Migration.Model
{
    public class LowloadedMigrationNode : MigrationNode
    {
        public LowloadedMigrationNode(MigrationRootNode root, VM target, Server reciever, bool turnOnNew) 
            : base(root, target, reciever, turnOnNew)
        {
        }

        public LowloadedMigrationNode(MigrationNode parent, VM target, Server reciever, bool turnOnNew = false)
            : base(parent, target, reciever, turnOnNew)
        {
        }

        // TODO: runs very long time. Probably could not finished in some cases
        // TODO: add additinal cases to exit, check childs count
        protected override bool CalculateValidity()
        {
            return Changes.Length == _root.TargetServer.RunningVMs.Count;
        }

        protected override float CalculateValue(IStateNode previous, bool newTurnOn)
        {
            if (_turnOnCount > 0)
            {
                return float.MinValue;  // our task is to turn off server not turn on
            }

            float val = previous.Value;

            val += GetTargetServerResourcesChange().GetValue();

            var change = Changes.Last();
            val += CalculateDiffForUpdatedMachineCase(change.Target.Resources + change.MigrationRequirment);

            return val;
        }

        private float CalculateDiffForUpdatedMachineCase(Resources serverResourcesChange)
        {
            var totalServers = _root.Recievers.Count() + _turnOnCount;
            return serverResourcesChange.GetValue() / totalServers;
        }

        protected override IStateNode CreateNode(MigrationNode parent, VM target, Server reciever, bool turnOnNew = false)
        {
            return new LowloadedMigrationNode(parent, target, reciever, turnOnNew);
        }

        public static MigrationNode FromRootNode(MigrationRootNode root, VM target, Server reciever, bool turnOnNew)
        {
            return new LowloadedMigrationNode(root, target, reciever, turnOnNew);
        }
    }
}
