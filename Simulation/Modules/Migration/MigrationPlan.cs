using Simulation.Models;
using Simulation.Modules.Migration.Model;
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

            public Server Reciever { get; }

            public int Time { get; }

            public PlanItem(VM targetVM, Server from, Server to)
            {
                Target = targetVM;
                Source = from;
                Reciever = to;
            }
        }

        private List<PlanItem> _planData = new List<PlanItem>();

        public MigrationPlan Merge(MigrationPlan other)
        {
            _planData.AddRange(other._planData);
            return this;
        }

        public void Add(MigrationNode node, Server target)
        {
            _planData.AddRange(
                node.Changes
                    .Select((change) => new PlanItem(
                                                change.Target,
                                                target,
                                                change.Reciever )));
        }

        public string GetFullInfo()
        {
            var sb = new StringBuilder("VM\tFrom\tTo\tEstimated time\n");

            foreach (var item in _planData)
            {
                sb.AppendLine($"{item.Target.Id}\t{item.Source.Id}\t{item.Reciever.Id}\t{item.Time}");
            }

            return sb.ToString();
        } 

        public string GetShortInfo()
        {
            return $"Migration Plan contains {_planData.Count} migrations";
        }

        public bool IsEmpty => _planData.Count == 0;

        private static readonly Lazy<MigrationPlan> _empty = new Lazy<MigrationPlan>();
        public static MigrationPlan Empty => _empty.Value;
    }
}
