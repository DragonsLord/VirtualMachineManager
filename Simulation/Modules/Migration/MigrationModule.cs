using Simulation.Models;
using Simulation.Models.Collections;
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
                .OrderBy(s => s.Resources.EvaluateVolume())
                .ThenBy(s => s.RunningVMs.Count)
                .ThenBy(s => s.PrognosedUsedResources[input.Depth].EvaluateVolume() / s.RunningVMs.Count)
                .ToList();

            var recievers = copies.Where(server => server.TurnedOn);
            var reserve = copies.Where(server => !server.TurnedOn);
            foreach (var server in input.Targets)
            {
                var resultNode = _searchEngine.Run(
                    new MigrationRootNode(
                        server,
                        recievers.ToList(),
                        reserve.ToList(),
                        input.Depth,
                        GetInitialValue(recievers, input.Depth, Evaluator.EvaluateForOverloading),
                        OverloadedMigrationNode.FromRootNode)
                    ) as MigrationNode;
                if (resultNode == null)
                    break;
                migrationPlan.Add(resultNode, server);
                foreach (var change in resultNode.Changes)
                {
                    copies.Find(s => s.Id == change.Reciever.Id).RunVM(change.Target);
                }
            }
            return migrationPlan;
        }

        public MigrationPlan ReleaseLowloadedMachines(DiagnosticResult input)
        {
            var migrationPlan = new MigrationPlan();
            var copies = input.Recievers
                .Select(server => server.ShalowCopy())
                .OrderBy(s => s.Resources.EvaluateVolume())
                .ThenBy(s => s.RunningVMs.Count)
                .ThenBy(s => s.PrognosedUsedResources[input.Depth].EvaluateVolume() / s.RunningVMs.Count)
                .ToList();

            var recievers = copies.Where(server => server.TurnedOn);
            var reserve = copies.Where(server => !server.TurnedOn);
            foreach (var server in input.Targets)
            {
                var resultNode = _searchEngine.Run(
                    new MigrationRootNode(
                        server,
                        recievers.ToList(),
                        reserve.ToList(),
                        input.Depth,
                        GetInitialValue(recievers, input.Depth, Evaluator.EvaluateForReleasing),
                        LowloadedMigrationNode.FromRootNode)
                    ) as MigrationNode;
                if (resultNode == null || !resultNode.IsValid)  // apply migration only if all VM is going to migrate
                    break;
                migrationPlan.Add(resultNode, server);
                server.MarkToShutdown();
                foreach (var change in resultNode.Changes)
                {
                    copies.Find(s => s.Id == change.Reciever.Id).RunVM(change.Target);
                }
            }
            return migrationPlan;
        }

        public List<MigrationTask> ApplayMigrations(MigrationPlan migrationPlan, ServerCollection servers)
        {
            var migrationTasks = new List<MigrationTask>(migrationPlan.Count);

            foreach (var migration in migrationPlan)
            {
                migrationTasks.Add(new MigrationTask(
                        migration.Target,
                        servers.Get(migration.SourceId),
                        servers.Get(migration.RecieverId)));
            }

            return migrationTasks;
        }

        private float GetInitialValue(IEnumerable<Server> recievers, byte depth, Func<Server, byte, float> evaluator)
        {
            if (recievers.Any())
            {
                return recievers.Average((s) => evaluator(s, depth));
            }
            else return 0;
        }
    }
}
