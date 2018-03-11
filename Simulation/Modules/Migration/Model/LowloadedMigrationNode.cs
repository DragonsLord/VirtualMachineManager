using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simulation.Models;
using Simulation.Modules.Diagnostic;
using Simulation.Modules.Migration.Interfaces;

namespace Simulation.Modules.Migration.Model
{
    public class LowloadedMigrationNode : MigrationNode
    {
        public LowloadedMigrationNode(MigrationRootNode root, VM target, Server reciever, bool turnOnNew) 
            : base(root, target, reciever, turnOnNew)
        {
        }

        public LowloadedMigrationNode(MigrationNode root, VM target, Server reciever, bool turnOnNew = false)
            : base(root, target, reciever, turnOnNew)
        {
        }

        protected override bool CalculateValidity()
        {
            return Changes.Length == _root.TargetServer.RunningVMs.Count;
        }

        protected override float CalculateValue()
        {
            if (_turnOnCount > 0)
            {
                return float.NegativeInfinity;  // our task is to turn of server not turn on
            }
            float val = 0f;
            
            val += GetTargetServerResourcesChange(_root.Depth).EvaluateVolume();

            // TODO: [high] add value of average resources usage on recievers !!!

            return val;
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
