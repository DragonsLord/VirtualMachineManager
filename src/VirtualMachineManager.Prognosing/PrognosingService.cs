using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.Prognosing.Algorythms;
using VirtualMachineManager.Prognosing.Models;

namespace VirtualMachineManager.Prognosing
{
    public class PrognosingService
    {
        const int BathSize = 30;
        struct ForecastCompositeKey
        {
            public int vmId;
            public Resource resource;
            public string method;

            public ForecastCompositeKey(int vmId, Resource res, string method)
            {
                this.vmId = vmId;
                this.resource = res;
                this.method = method;
            }
        }

        delegate void WriteToResources(ref Resources s, float value);

        private readonly ISeriesStorage seriesStorage;
        private readonly RForecast rForecast;
        private long traceWindowTime = 0;

        // TODO: respect prognosing depth
        private Dictionary<ForecastCompositeKey, IEnumerable<ForecastResult>> lastForecasts
            = new Dictionary<ForecastCompositeKey, IEnumerable<ForecastResult>>();

        public PrognosingParams Params { get; }

        public PrognosingService(PrognosingParams @params, ISeriesStorage seriesStorage)
        {
            Params = @params;
            rForecast = new RForecast(@params);
            this.seriesStorage = seriesStorage;
        }

        public bool CanPrognose(int step) =>
            Params.PrognoseDepth > 0 && step > Params.MinTraceWindow;

        public IEnumerable<VMPrognose> Forecast(IEnumerable<VM> vms, int simulationStep)
        {
            void FillResources(Resources[] resources, float[] forecasts, WriteToResources fill)
            {
                for (int i = 0; i < resources.Length; i++)
                    fill(ref resources[i], forecasts[i]);
            }

            var vmsCount = vms.Count();
            var batchesCount = vmsCount % BathSize == 0 ? vmsCount / BathSize : vmsCount / BathSize + 1;
            var timer = new Stopwatch();

            CalculateTraceWindowTime(simulationStep);

            timer.Start();

            var r = Enumerable.Range(0, batchesCount)
                .SelectMany(i => rForecast.RunAlgorythms(vms.Skip(i * BathSize).Take(BathSize).SelectMany(MapToResourceTraces)))
                .ToList()
                .GroupBy(trace => trace.VmId)
                .Select(group =>
                {
                    Resources[] prognoses = new Resources[Params.PrognoseDepth];
                    Array.Fill(prognoses, new Resources());
                    var vm = vms.First(vm => vm.Id == group.Key);

                    foreach (var resourceForecast in group)
                    {
                        switch (resourceForecast.Resource)
                        {
                            case Resource.Cpu: FillResources(
                                prognoses,
                                GetForecast(resourceForecast, vm.Resources.CPU),
                                (ref Resources r, float f) => r.CPU = f
                            ); break;
                            case Resource.Network: FillResources(
                                prognoses,
                                GetForecast(resourceForecast, vm.Resources.Network),
                                (ref Resources r, float f) => r.Network = f
                            ); break;
                            case Resource.Memory: FillResources(
                                prognoses,
                                GetForecast(resourceForecast, vm.Resources.Memmory),
                                (ref Resources r, float f) => r.Memmory = f
                            ); break;
                            case Resource.Iops: FillResources(
                                prognoses,
                                GetForecast(resourceForecast, vm.Resources.IOPS),
                                (ref Resources r, float f) => r.IOPS = f
                            ); break;
                        }
                    }
                    return new VMPrognose(vm, prognoses);
                }).AsParallel().ToList();

            timer.Stop();

            Debug.WriteLine($"Step: {simulationStep}: {vms.Count()} vms forecasted in {timer.ElapsedMilliseconds}ms");
            return r;
        }

        public void UpdateTraces(IEnumerable<VM> vms, int timeId) =>
            seriesStorage.PushNextRecord(vms, timeId).Wait();
        
        private float[] GetForecast(VmResourceForecast forecast, float realValue)
        {
            var values = forecast.Forecast.Select(pair => {
                var key = new ForecastCompositeKey(forecast.VmId, forecast.Resource, pair.Key);

                if (!lastForecasts.ContainsKey(key))
                {
                    lastForecasts.Add(key, pair.Value);
                    return pair.Value.First();
                }
                int? bestOffset = null;
                float minErr = float.MaxValue, currErr = 0;

                // filtering only for available offsets
                foreach (var fr in lastForecasts[key].Where(r => pair.Value.Any(x => x.WindowOffset == r.WindowOffset)))
                {
                    // TODO: MAPE ?
                    /***
                     * MAPE will not work for 0 values.
                     * In our context we can continue using MAE as we basically camparing result for same series
                     * and we do not need to worry about value scaling and normalization
                     * ***/
                    currErr = Math.Abs(fr.Result[0] - realValue);
                    if (currErr < minErr)
                    {
                        minErr = currErr;
                        bestOffset = fr.WindowOffset;
                    }
                }

                lastForecasts[key] = pair.Value;

                return bestOffset.HasValue ? pair.Value.First(x => x.WindowOffset == bestOffset) : pair.Value.First();
            });
            
            return Enumerable.Range(0, Params.PrognoseDepth).Select(i => values.Select(arr => arr.Result[i]).Average()).ToArray();
        }

        private void CalculateTraceWindowTime(int simulationStep)
        {
            if (simulationStep > Params.MaxTraceWindow) ++traceWindowTime;
        }

        private IEnumerable<VmResourceTrace> MapToResourceTraces(VM vm)
        {
            var resourcesTrace = seriesStorage.GetVMTrace(vm.Id, traceWindowTime).Result;

            return new VmResourceTrace[]
            {
                new VmResourceTrace(vm.Id, Resource.Cpu, resourcesTrace.Select(r => r.CPU)),
                new VmResourceTrace(vm.Id, Resource.Memory, resourcesTrace.Select(r => r.Memmory)),
                new VmResourceTrace(vm.Id, Resource.Network, resourcesTrace.Select(r => r.Network)),
                new VmResourceTrace(vm.Id, Resource.Iops, resourcesTrace.Select(r => r.IOPS)),
            };
        }
    }
}
