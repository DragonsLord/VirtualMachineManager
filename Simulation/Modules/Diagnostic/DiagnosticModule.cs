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
            // TODO: add prognosed steps after prognose module implementation
            var overloaded = collection.Where(server => Evaluator.IsOverloaded(server, 0));
            return new DiagnosticResult(
                overloaded,
                PrepareRecievers(collection, 0),
                0 );
        }

        public DiagnosticResult DetectLowloadedMachines(ServerCollection collection)
        {
            // TODO: implement
            return DiagnosticResult.Empty;
        }

        private IEnumerable<Server> PrepareRecievers(IEnumerable<Server> servers, byte depth)
        {
            var recievers = servers.Where(s => ValidateThreadhold(s, depth));

            return recievers;
        }

        private bool ValidateThreadhold(Server s, byte depth)
        {
            var freeRes = s.Resources - s.PrognosedUsedResources[depth];
            return s.TurnedOn || (
                freeRes.CPU > GlobalConstants.CPU_RECIEVER_THREADHOLD * s.Resources.CPU &&
                freeRes.Memmory > GlobalConstants.MEMMORY_RECIEVER_THREADHOLD * s.Resources.Memmory &&
                freeRes.IOPS > GlobalConstants.IOPS_RECIEVER_THREADHOLD * s.Resources.IOPS &&
                freeRes.Network > GlobalConstants.NETWORK_RECIEVER_THREADHOLD * s.Resources.Network);
        }
    }
}
