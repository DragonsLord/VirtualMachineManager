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

        public OverloadedMigrationNode(MigrationNode root, VM target, Server reciever, bool turnOnNew = false)
            : base(root, target, reciever, turnOnNew)
        {
        }

        protected override bool CalculateValidity()
        {
            for (byte i = 0; i <= _root.Depth; i++)  // TODO: is cycle neccessary ?
            {
                if (Evaluator.IsOverloaded(
                    _root.TargetServer.PrognosedUsedResources[i] - GetTargetServerResourcesChange(i),
                    _root.TargetServer))
                    return false;
            }
            return true;
        }

        protected override float CalculateValue()
        {
            float val = 0f;

            val -= _turnOnCount * GlobalConstants.TURN_ON_SHTRAF;

            val += GetTargetServerResourcesChange(_root.Depth).EvaluateVolume();

            // TODO: add value of average resources usage on recievers ?

            return val;
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
