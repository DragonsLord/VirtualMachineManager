﻿using System;
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
        delegate void WriteToResources(ref Resources s, float value);

        private readonly ISeriesStorage seriesStorage;
        private readonly RForecast rForecast;
        private long traceWindowTime = 0;
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

            var timer = new Stopwatch();

            CalculateTraceWindowTime(simulationStep);

            timer.Start();

            var r = rForecast.RunAlgorythms(vms.SelectMany(MapToResourceTraces))
                .GroupBy(trace => trace.VmId)
                .Select(group =>
                {
                    Resources[] prognoses = new Resources[Params.PrognoseDepth];
                    Array.Fill(prognoses, new Resources());
                    foreach (var resourceForecast in group)
                    {
                        switch (resourceForecast.Resource)
                        {
                            case Resource.Cpu: FillResources(prognoses, GetForecast(resourceForecast), (ref Resources r, float f) => r.CPU = f); break;
                            case Resource.Network: FillResources(prognoses, GetForecast(resourceForecast), (ref Resources r, float f) => r.Network = f); break;
                            case Resource.Memory: FillResources(prognoses, GetForecast(resourceForecast), (ref Resources r, float f) => r.Memmory = f); break;
                            case Resource.Iops: FillResources(prognoses, GetForecast(resourceForecast), (ref Resources r, float f) => r.IOPS = f); break;
                        }
                    }
                    return new VMPrognose(vms.First(vm => vm.Id == group.Key), prognoses);
                });

            timer.Stop();

            Debug.WriteLine($"Step: {simulationStep}: {vms.Count()} vms forecasted in {timer.ElapsedMilliseconds}ms");
            return r;
        }

        public void UpdateTraces(IEnumerable<VM> vms, int timeId) =>
            seriesStorage.PushNextRecord(vms, timeId).Wait();

        private float[] GetForecast(VmResourceForecast forecast)
        {
            var values = forecast.Forecast.Select(pair => pair.Value);
            return Enumerable.Range(0, Params.PrognoseDepth).Select(i => values.Select(arr => arr[i]).Average()).ToArray();
        }

        private void CalculateTraceWindowTime(int simulationStep)
        {
            if (simulationStep > Params.MaxTraceWindow) traceWindowTime++;
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
