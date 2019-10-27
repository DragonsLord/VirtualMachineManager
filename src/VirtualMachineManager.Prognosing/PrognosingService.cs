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
        private readonly RForecast rForecast;
        public PrognosingParams Params { get; }

        public PrognosingService(PrognosingParams @params, ISeriesStorage seriesStorage)
        {
            Params = @params;
            this.seriesStorage = seriesStorage;

            rForecast = new RForecast(Params.PrognoseDepth);
        }

        public IEnumerable<VMPrognose> Forecast(IEnumerable<VM> vms) =>
            vms.Select(ForecastVM);

        public void UpdateTraces(IEnumerable<VM> vms, long time)
        {
            seriesStorage.PushNextRecord(vms, time).Wait();
        }

        private VMPrognose ForecastVM(VM vm)
        {
            var resourcesTrace = seriesStorage.GetVMTrace(vm.Id).Result;

            var cpuPrediction = GetTracePrediction(
                "cpu" + vm.Id,
                resourcesTrace.Select(r => (double)r.CPU + 1),
                vm.Resources.CPU + 1);

            var memoryPrediction = GetTracePrediction(
                "memory" + vm.Id,
                resourcesTrace.Select(r => (double)r.Memmory + 1),
                vm.Resources.Memmory + 1);

            var iopsPrediction = GetTracePrediction(
                "iops" + vm.Id,
                resourcesTrace.Select(r => (double)r.IOPS + 1),
                vm.Resources.IOPS + 1);

            var networkPrediction = GetTracePrediction(
                "network" + vm.Id,
                resourcesTrace.Select(r => (double)r.Network + 1),
                vm.Resources.Network + 1);

            return new VMPrognose(vm,
                Enumerable.Range(0, Params.PrognoseDepth)
                .Select(i => new Resources()
                {
                    CPU = (float)cpuPrediction[i] - 1,
                    Network = (float)networkPrediction[i] - 1,
                    Memmory = (float)memoryPrediction[i] - 1,
                    IOPS = (float)iopsPrediction[i] - 1,
                }).ToArray());
        }

        private double[] GetTracePrediction(string traceId, IEnumerable<double> trace, double realValue)
        {
            var mape = rForecast.GetMAPE(traceId, realValue);
            var predictions = rForecast.RunAlgorythms(traceId, trace);

            double minMape = double.PositiveInfinity;
            string bestAlgorythm = "";

            foreach (var pair in mape)
            {
                if (pair.Value < minMape)
                {
                    minMape = pair.Value;
                    bestAlgorythm = pair.Key;
                }
            }

            return predictions[bestAlgorythm];
        }
    }
}
