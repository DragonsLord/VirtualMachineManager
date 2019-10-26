using System.Collections.Generic;

namespace VirtualMachineManager.Prognosing.Models
{
    public interface IForcastAlgorythm
    {
        public T[] Forecast<T>(IEnumerable<T> series, int amountToPredict);
    }
}
