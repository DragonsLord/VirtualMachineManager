using Simulation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Modules.Migration
{
    public class MigrationModule
    {
        public MigrationPlan MigrateFromOverloaded(IEnumerable<Server> overloadedMachines)
        {
            // TODO: implement
            return MigrationPlan.Empty;
        }

        public MigrationPlan ReleaseLowloadedMachines(IEnumerable<Server> lowloadedMachines)
        {
            // TODO: implement
            return MigrationPlan.Empty;
        }
    }
}
