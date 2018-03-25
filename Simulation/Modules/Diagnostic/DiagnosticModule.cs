using Simulation.Models;
using Simulation.Models.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Simulation.Modules.Diagnostic
{
    public class DiagnosticModule
    {
        public DiagnosticResult DetectOverloadedMachines(ServerCollection collection)
        {
            byte depth = 0;
            Logger.StartAction("Diagnosting for overloaded servers");
            // TODO: add prognosed steps after prognose module implementation
            var overloaded = collection.Where(server => !server.InMigration && Evaluator.IsOverloaded(server, 0));
            var result = new DiagnosticResult(
                overloaded,
                collection.Where(s => ValidateThreadhold(s, depth, true)),
                depth);
            Logger.EndAction();
            return result;
        }

        public DiagnosticResult DetectLowloadedMachines(ServerCollection collection)
        {
            byte depth = 0;
            // TODO: record ordering herustic
            var lowLoaded = collection.Where((server) => !server.InMigration && Evaluator.IsLowLoaded(server, depth))
                .OrderByDescending((server) => server.PrognosedUsedResources[depth].EvaluateVolume())
                .ThenBy((server) => server.RunningVMs.Count);

            if (!lowLoaded.Any())
            {
                return DiagnosticResult.Empty;
            }

            var recievers = collection.Where((server) => !lowLoaded.Any((s) => s.Id == server.Id))
                .Where(s => ValidateThreadhold(s, depth, false));

            if (!recievers.Any())
            {
                return DiagnosticResult.Empty;
            }

            var totalFree = recievers.Select(s => s.Resources - s.PrognosedUsedResources[depth])
                .Aggregate((r1, r2) => r1 + r2);

            totalFree -= new Resources()
            {
                CPU = totalFree.CPU * GlobalConstants.CPU_THREADHOLD,
                Memmory = totalFree.Memmory * GlobalConstants.MEMMORY_THREADHOLD,
                Network = totalFree.Network * GlobalConstants.NETWORK_THREADHOLD,
                IOPS = totalFree.IOPS * GlobalConstants.IOPS_THREADHOLD
            };

            return new DiagnosticResult(
                lowLoaded.Where((server) => (totalFree -= server.PrognosedUsedResources[depth]) > 0),
                recievers,
                depth);
        }

        private bool ValidateThreadhold(Server s, byte depth, bool includeOffline)
        {
            var freeRes = s.Resources - s.PrognosedUsedResources[depth];
            return !s.InMigration && !Evaluator.IsOverloaded(s, depth) && ((!s.TurnedOn && includeOffline) || (
                freeRes.CPU > GlobalConstants.CPU_RECIEVER_THREADHOLD * s.Resources.CPU &&
                freeRes.Memmory > GlobalConstants.MEMMORY_RECIEVER_THREADHOLD * s.Resources.Memmory &&
                freeRes.IOPS > GlobalConstants.IOPS_RECIEVER_THREADHOLD * s.Resources.IOPS &&
                freeRes.Network > GlobalConstants.NETWORK_RECIEVER_THREADHOLD * s.Resources.Network));
        }
    }
}
