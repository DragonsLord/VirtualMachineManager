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

        protected override bool CalculateValidity()
        {
            for (byte i = 0; i <= _root.Depth; i++)  // TODO: is cycle neccessary ?
            {
                if (Evaluator.IsOverloaded(
                    _root.TargetServer.PrognosedUsedResources[i] + GetTargetServerResourcesChange(i),
                    _root.TargetServer))
                    return false;
            }
            return true;
        }

        protected override float CalculateValue(IStateNode previous, bool newTurnOn)
        {
            float val = previous.Value;
            
            val += GetTargetServerResourcesChange(_root.Depth).EvaluateVolume();

            var change = Changes.LastItem();
            var newServerResources = GetRecieverResources(change.Reciever, _root.Depth);

            if (newTurnOn) {
                val -= GlobalConstants.TURN_ON_PENALTY;
                val += CalculateDiffForNewMachineCase(previous.Value, newServerResources);
            } else {
                val += CalculateDiffForUpdatedMachineCase(change.Target.Resources);
            }

            return val;
        }

        private float CalculateDiffForUpdatedMachineCase(Resources serverResourcesChange)
        {
            var totalServers = _root.Recievers.Count() + _turnOnCount;
            return serverResourcesChange.EvaluateVolume() / totalServers;
        }

        private float CalculateDiffForNewMachineCase(float oldValue, Resources newServerValue)
        {
            var totalServers = _root.Recievers.Count() + _turnOnCount;
            return (newServerValue.EvaluateVolume() - oldValue) / totalServers;
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
