using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Modules.Migration
{
    public class MigrationPlan
    {
        // structure: MachineId | VMId | Requirments (time and etc)
        //TODO: Use Matrix or List (Gues: matrix better for merging?)

        public MigrationPlan Merge(MigrationPlan other)
        {
            // TODO: implement
            return Empty;
        }

        public void SaveToFile()
        {
            //TODO: implement
        } 

        public string GetShortInfo()
        {
            // TODO: implement
            return "";
        }

        private static readonly Lazy<MigrationPlan> _empty = new Lazy<MigrationPlan>();
        public static MigrationPlan Empty => _empty.Value;
    }
}
