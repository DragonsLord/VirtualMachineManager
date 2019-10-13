using Simulation.Modules.Migration.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.Migration
{
    public class MigrationPlan: IEnumerable<MigrationPlan.Item>
    {
        public class Item
        {
            public VM Target { get; }

            public int SourceId { get; }

            public int RecieverId { get; }

            public int Time { get; }

            public Item(VM targetVM, Server from, Server to)
            {
                Target = targetVM;
                SourceId = from.Id;
                RecieverId = to.Id;
            }
        }

        private List<Item> _planData = new List<Item>();

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
                    .Select((change) => new Item(
                                                change.Target,
                                                target,
                                                change.Reciever )));
        }

        /*public string GetFullInfo()
        {
            var sb = new StringBuilder("VM\tFrom\tTo\n");

            foreach (var item in _planData)
            {
                sb.AppendLine($"{Logger.Indent}{item.Target.Id}\t{item.SourceId}\t{item.RecieverId}");
            }

            return sb.ToString();
        }*/ 

        public string GetShortInfo()
        {
            return $"Migration Plan contains {_planData.Count} migrations";
        }

        public IEnumerator<Item> GetEnumerator() => _planData.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _planData.GetEnumerator();

        public bool IsEmpty => _planData.Count == 0;

        private static readonly Lazy<MigrationPlan> _empty = new Lazy<MigrationPlan>();
        public static MigrationPlan Empty => _empty.Value;
    }
}
