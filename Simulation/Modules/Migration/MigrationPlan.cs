using Simulation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Modules.Migration
{
    public class MigrationPlan
    {
        // structure: VMId | MachineIds | Requirments (time and etc)
        class PlanItem
        {
            public VM Target { get; }

            public Server Source { get; }

            public Server Recipient { get; }

            public int Time { get; }

            public PlanItem(VM targetVM, Server from, Server to)
            {
                Target = targetVM;
                Source = from;
                Recipient = to;
            }
        }

        private List<PlanItem> _planData = new List<PlanItem>();

        public MigrationPlan Merge(MigrationPlan other)
        {
            _planData.AddRange(other._planData);
            return this;
        }

        public string GetFullInfo()
        {
            var sb = new StringBuilder("VM\tFrom\tTo\tEstimated time\n");

            foreach (var item in _planData)
            {
                sb.AppendLine($"{item.Target.Id}\t{item.Source.Id}\t{item.Recipient.Id}\t{item.Time}");
            }

            return sb.ToString();
        } 

        public string GetShortInfo()
        {
            return $"Migration Plan contains {_planData.Count} migrations";
        }

        private static readonly Lazy<MigrationPlan> _empty = new Lazy<MigrationPlan>();
        public static MigrationPlan Empty => _empty.Value;
    }
}
