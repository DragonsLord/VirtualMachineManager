using System;
using System.Collections;
using System.Collections.Generic;
using Utilities;

namespace Simulation.Modules.Prognosing.Model
{
    public class StatisticalDataStream<T>: IEnumerable<T>
    {
        private T[] _valuesContainer;

        private int _head;
        private int _tail;

        public int Count { get; private set; } = 0;

        public StatisticalDataStream(int amount = GlobalConstants.MAX_VALUES_AMOUNT)
        {
            amount = Math.Max(amount, GlobalConstants.INDEPENDENT_VALUES_AMOUNT * 2);
            _valuesContainer = new T[amount];
            _head = amount - 1;
            _tail = _head;
        }

        // TODO: fix (consider add tail as property instead of calculationg it every time (it`s broken right now))
        public void Push(T value)
        {
            _valuesContainer[_tail] = value;
            _head = _tail;
            if (--_tail < 0)
            {
                _tail = _valuesContainer.Length - 1;
            }
            Count = Math.Min(Count + 1, _valuesContainer.Length);
        }

        public IEnumerable<T> GetPartial(int offset, int amount = GlobalConstants.INDEPENDENT_VALUES_AMOUNT)
        {
            for (int i = 0; i < amount; i++)
            {
                yield return _valuesContainer[(_head + i + offset) % _valuesContainer.Length];
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _valuesContainer[(_head + i) % _valuesContainer.Length];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
