using System;
using System.Collections.Generic;
using System.Linq;
using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.Diagnostics.Models
{
    public class DiagnosticResult
    {
        public IEnumerable<Server> Targets { get; }

        public IEnumerable<Server> Recievers { get; }

        public DiagnosticResult(IEnumerable<Server> targets, IEnumerable<Server> recievers)
        {
            Targets = targets;
            Recievers = recievers;
        }

        public static DiagnosticResult Empty = new DiagnosticResult(Array.Empty<Server>(), Array.Empty<Server>());
    }
}
