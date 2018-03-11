using Simulation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Modules.Diagnostic
{
    public class DiagnosticResult
    {
        public IEnumerable<Server> Targets { get; }

        public IEnumerable<Server> Recievers { get; }

        public byte Depth { get; set; }

        public bool AreOverloadedMachines => Targets.Any();

        public DiagnosticResult(IEnumerable<Server> targets, IEnumerable<Server> recievers, byte depth)
        {
            Targets = targets;
            Recievers = recievers;
            Depth = depth;
        }
    }
}
