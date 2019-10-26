using System.Collections.Generic;
using System.Linq;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.Prognosing.Algorythms;
using VirtualMachineManager.Prognosing.Models;

namespace VirtualMachineManager.Prognosing
{
    public class PrognosingService
    {
        private readonly ISeriesStorage seriesStorage;

        private readonly Dictionary<AlgorythmType, IForcastAlgorythm> algorythms = new Dictionary<AlgorythmType, IForcastAlgorythm>();

        public PrognosingParams Params { get; }

        public PrognosingService(PrognosingParams @params, ISeriesStorage seriesStorage)
        {
            Params = @params;
            this.seriesStorage = seriesStorage;
        }

        public IEnumerable<VMPrognose> Forecast(IEnumerable<VM> vms)
        {
            return Enumerable.Empty<VMPrognose>();
        }

        public void UpdateTraces(IEnumerable<VM> vms, long time)
        {
            seriesStorage.PushNextRecord(vms, time).Wait();
        }
    }
}
