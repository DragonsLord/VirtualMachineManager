using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtualMachineManager.Migration.Algorythm.Interfaces;

namespace VirtualMachineManager.Migration.Algorythm
{
    [Serializable]
    public class BeamSearchAlgorythm
    {
        private int _beam_lenth = int.MaxValue;
        public int BeamLenth {
            get => _beam_lenth;
            set => _beam_lenth = value;
        }

        private class Beam : IEnumerable<IStateNode>
        {
            public readonly int Size;
            private SortedSet<IStateNode> _beam;
            private object _lock = new object();

            public Beam(int beam_size, IComparer<IStateNode> comp)
            {
                Size = beam_size;
                _beam = new SortedSet<IStateNode>(comp);
            }

            public void Add(IStateNode element)
            {
                Monitor.Enter(_lock);
                _beam.Add(element);
                if (_beam.Count > Size)
                    _beam.Remove(_beam.Last());
                Monitor.Exit(_lock);
            }

            public IEnumerable<IStateNode> Select => _beam.Take(Size).ToList();

            public void Clear() => _beam.Clear();

            public int Count => _beam.Count;

            public IEnumerator<IStateNode> GetEnumerator()
            {
                return ((IEnumerable<IStateNode>)_beam).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable<IStateNode>)_beam).GetEnumerator();
            }
        }

        public IComparer<IStateNode> Comparer { get; set; }

        private IEnumerable<IStateNode> _layer;

        public BeamSearchAlgorythm(int beam_lenth): this(beam_lenth, new AscendingStateComparer())
        { }

        public BeamSearchAlgorythm(int beam_lenth, IComparer<IStateNode> comp)
        {
            _beam_lenth = beam_lenth;
            Comparer = comp;
        }

        public IStateNode Run(IStateNode root)
        {
            var beam = new Beam(_beam_lenth, Comparer);
            beam.Add(root);
            while (beam.Count > 0)
            {
                var start = DateTime.Now;
                _layer = beam.ToList();
                beam.Clear();
                var result = _layer.FirstOrDefault(node => node.IsValid);
                if (result != null) return result;
                Parallel.ForEach(_layer, el =>
                {
                    foreach (var child in el.GetChilds().OrderByDescending(s => s.Value).Take(_beam_lenth))
                        beam.Add(child);
                });
            }
            return _layer.First();
        }
    }
}
