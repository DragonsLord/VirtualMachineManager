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

        public IEnumerable<Server> Recipients { get; }

        public int Depth { get; set; }

        public DiagnosticResult(IEnumerable<Server> targets, IEnumerable<Server> recipients, int depth)
        {
            Targets = targets;
            Recipients = recipients;
            Depth = depth;
        }

        public static DiagnosticResult Empty => new DiagnosticResult(new Server[0], new Server[0], 0);      // TODO: think about it
    }
}
