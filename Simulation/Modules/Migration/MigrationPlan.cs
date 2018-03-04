using Simulation.Models;
using Simulation.Modules.Migration.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Modules.Migration
{
    public class MigrationPlan: IEnumerable<MigrationPlan.PlanItem>
    {
        public class PlanItem
        {
            public VM Target { get; }

            public int SourceId { get; }

            public int RecieverId { get; }

            public int Time { get; }

            public PlanItem(VM targetVM, Server from, Server to)
            {
                Target = targetVM;
                SourceId = from.Id;
                RecieverId = to.Id;
            }
        }

        private List<PlanItem> _planData = new List<PlanItem>();

        public int Count => _planData.Count;

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
                sb.AppendLine($"{item.Target.Id}\t{item.SourceId}\t{item.RecieverId}\t{item.Time}");
            }

            return sb.ToString();
        } 

        public string GetShortInfo()
        {
            return $"Migration Plan contains {_planData.Count} migrations";
        }

        public IEnumerator<PlanItem> GetEnumerator()
        {
            return ((IEnumerable<PlanItem>)_planData).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<PlanItem>)_planData).GetEnumerator();
        }

        public bool IsEmpty => _planData.Count == 0;

        private static readonly Lazy<MigrationPlan> _empty = new Lazy<MigrationPlan>();
        public static MigrationPlan Empty => _empty.Value;
    }
}
