using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulation.Models;
using Simulation.Modules.Diagnostic;
using Simulation.Modules.Migration.Interfaces;
using Utilities;

namespace Simulation.Modules.Migration.Model
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

        protected override bool CalculateValidity()
        {
            return Changes.Length == _root.TargetServer.RunningVMs.Count;
        }

        protected override float CalculateValue(IStateNode previous, bool newTurnOn)
        {
            if (_turnOnCount > 0)
            {
                // TODO: Consider MinValue instead of Infinity
                return float.NegativeInfinity;  // our task is to turn off server not turn on
            }

            float val = previous.Value;

            val += GetTargetServerResourcesChange(_root.Depth).EvaluateVolume();

            var change = Changes.LastItem();
            val += CalculateDiffForUpdatedMachineCase(change.Target.PrognosedResources[_root.Depth]);

            return val;
        }

        private float CalculateDiffForUpdatedMachineCase(Resources serverResourcesChange)
        {
            var totalServers = _root.Recievers.Count() + _turnOnCount;
            return serverResourcesChange.EvaluateVolume() / totalServers;
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
