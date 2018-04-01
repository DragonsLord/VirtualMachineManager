using System;
using System.Collections;
using System.Collections.Generic;
using Utilities;

namespace Simulation.Modules.Prognosing.Model
{
    public class StatisticalDataStream<T>: IEnumerable<T>
    {
        private T[] _valuesContainer;

        private int _head = 0;

        public int Count { get; private set; }

        private int GetTail()
        {
            if (Count == 0)
            {
                return _head;
            }
            if (Count < _valuesContainer.Length)
            {
                return (_head + Count) % _valuesContainer.Length;
            }
            if (_head == 0)
            {
                return _valuesContainer.Length - 1;
            }
            return _head - 1;
        }

        public StatisticalDataStream(int amount = GlobalConstants.INDEPENDENT_VALUES_AMOUNT)
        {
            _valuesContainer = new T[amount * 2];
            Count = 0;
            _head = amount - 1;
        }

        public void Push(T value)
        {
            var tail = GetTail();
            _valuesContainer[tail] = value;
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
