using Simulation.Models;
using Simulation.Modules.Diagnostic;
using Simulation.Modules.Migration.Algorythm;
using Simulation.Modules.Migration.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Simulation.Modules.Migration
{
    public class MigrationModule
    {
        private BeamSearchAlgorythm _searchEngine = new BeamSearchAlgorythm(GlobalConstants.BEAM_LENTH);

        public MigrationPlan MigrateFromOverloaded(DiagnosticResult input)
        {
            var migrationPlan = new MigrationPlan();
            var copies = input.Recievers
                .Select(server => server.ShalowCopy())
                .ToList();
            // TODO: check if expressions recalculate each time
            var recievers = copies.Where(server => server.TurnedOn);
            var reserve = copies.Where(server => !server.TurnedOn);
            foreach (var server in input.Targets)
            {
                var resultNode = _searchEngine.Run(
                    new MigrationRootNode(server, recievers.ToList(), reserve.ToList(), input.Depth)
                    ) as MigrationNode;
                migrationPlan.Add(resultNode, server);
                foreach (var change in resultNode.Changes)
                {
                    copies.Find(s => s.Id == change.Reciever.Id).RunVM(change.Target);
                }
            }
            return migrationPlan;
        }

        public MigrationPlan ReleaseLowloadedMachines(IEnumerable<Server> lowloadedMachines)
        {
            // TODO: implement (try use same code but with different Evaluation functions)
            return MigrationPlan.Empty;
        }
    }
}
