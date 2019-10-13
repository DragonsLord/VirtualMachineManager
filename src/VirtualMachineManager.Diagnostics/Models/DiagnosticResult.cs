using System.Collections.Generic;
using System.Linq;
using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.Diagnostics.Models
{
    public class DiagnosticResult
    {
        public IEnumerable<Server> Targets { get; }

        public IEnumerable<Server> Recievers { get; }

        // public bool AreTargetMachines => Targets.Any();

        public DiagnosticResult(IEnumerable<Server> targets, IEnumerable<Server> recievers)
        {
            Targets = targets;
            Recievers = recievers;
        }

        //public static DiagnosticResult Empty => new DiagnosticResult(new Server[0], null, 0);
    }
}
