using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.Prognosing.Algorythms;
using VirtualMachineManager.Services;

namespace VirtualMachineManager.PrognosingStatistics
{
    class Program
    {
        static void Main(string[] args)
        {
            string rPackagesPath = "rPackages";
            if (!Directory.Exists(rPackagesPath))
            {
                Directory.CreateDirectory(rPackagesPath);
            }
            var settings = new Prognosing.Models.PrognosingParams()
            {
                PrognoseDepth = 1,
                MinTraceWindow = 20,
                MaxTraceWindow = 25
            };
            var steps = 5000;

            string inputFolder = args.Any() ? args[0] : "Input";

            string vm1TracePath = Path.Combine(inputFolder, "Traces", "1.csv");

            var trace = ReadTrace(new FileInfo(vm1TracePath)).Take(steps).ToArray();

            var traceSerieses = Enumerable.Range(0, trace.Count() - settings.MinTraceWindow - 1)
                .Select(i => trace.Skip(i).Take(settings.MaxTraceWindow))
                .SelectMany(series => 
                    new VmResourceTrace[]
                    {
                        new VmResourceTrace(1, Resource.Cpu, series.Select(r => r.CPU)),
                        new VmResourceTrace(1, Resource.Network, series.Select(r => r.Network)),
                        new VmResourceTrace(1, Resource.Memory, series.Select(r => r.Memmory)),
                        new VmResourceTrace(1, Resource.Iops, series.Select(r => r.IOPS))
                    });

            var reportService = new ReportService("vmPrognose.xlsx", settings.PrognoseDepth);
            reportService.InitVmPrognoseSheets(1, "ARIMA", "SES", "HOLT", "DHOLT", "CROST");

            var vm = new VM() { Id = 1 };
            var previousForecast = new Dictionary<Resource, Dictionary<string, IEnumerable<ForecastResult>>>()
            {
                { Resource.Cpu, null },
                { Resource.Network, null },
                { Resource.Memory, null },
                { Resource.Iops, null }
            };
            var stat = new Dictionary<Resource, Dictionary<string, float[]>>()
            {
                { Resource.Cpu, new Dictionary<string, float[]>() },
                { Resource.Network, new Dictionary<string, float[]>() },
                { Resource.Memory, new Dictionary<string, float[]>() },
                { Resource.Iops, new Dictionary<string, float[]>() }
            };

            try
            {
                RGlobalEnvironment.InitREngineWithForecasing(rPackagesPath);
                RForecast rForecast = new RForecast(settings);

                for (int i = 0; i < steps; i++)
                {
                    Console.WriteLine($"step {i}");
                    vm.Resources = trace[i];


                    if (i >= settings.MinTraceWindow - 1)
                    {
                        var series = trace.Skip(Math.Max(i - settings.MaxTraceWindow + 1, 0)).Take(settings.MaxTraceWindow).ToArray();
                        var results = rForecast.RunAlgorythms(
                                new VmResourceTrace[]
                                {
                                    new VmResourceTrace(1, Resource.Cpu, series.Select(r => r.CPU)),
                                    new VmResourceTrace(1, Resource.Network, series.Select(r => r.Network)),
                                    new VmResourceTrace(1, Resource.Memory, series.Select(r => r.Memmory)),
                                    new VmResourceTrace(1, Resource.Iops, series.Select(r => r.IOPS))
                                }).ToDictionary(f => f.Resource, f => f.Forecast);

                        stat[Resource.Cpu] = results[Resource.Cpu].ToDictionary(p => p.Key,
                            p => p.Value.First(
                                f => f.WindowOffset == GetBest(p.Key, previousForecast[Resource.Cpu], trace[i].CPU)).Result);
                        stat[Resource.Network] = results[Resource.Network].ToDictionary(p => p.Key,
                            p => p.Value.First(
                                f => f.WindowOffset == GetBest(p.Key, previousForecast[Resource.Network], trace[i].Network)).Result);
                        stat[Resource.Memory] = results[Resource.Memory].ToDictionary(p => p.Key,
                            p => p.Value.First(
                                f => f.WindowOffset == GetBest(p.Key, previousForecast[Resource.Memory], trace[i].Memmory)).Result);
                        stat[Resource.Iops] = results[Resource.Iops].ToDictionary(p => p.Key,
                            p => p.Value.First(
                                f => f.WindowOffset == GetBest(p.Key, previousForecast[Resource.Iops], trace[i].IOPS)).Result);

                        previousForecast = results;
                    }

                    reportService.WriteVmPrognoseStatistics(i + 1, vm, stat);
                }


                reportService.DrawVmForecastCharts();
                reportService.Save();
            }
            finally
            {
                RGlobalEnvironment.R.Dispose();
                reportService.Dispose();
            }
        }

        private static int GetBest(string method, Dictionary<string, IEnumerable<ForecastResult>> forecast, float actual)
        {
            if (forecast == null || !forecast.Any()) return 0;
            ForecastResult best = null;
            float minErr = float.MaxValue;

            foreach (var fr in forecast[method])
            {
                // TODO: MAPE ?
                var currErr = Math.Abs(fr.Result[0] - actual);
                if (currErr < minErr)
                {
                    minErr = currErr;
                    best = fr;
                }
            }

            return best?.WindowOffset ?? 0;
        }

        public static IEnumerable<Resources> ReadTrace(FileInfo trace)
        {
            var vmTrace = new List<Resources>();

            using (StreamReader reader = new StreamReader(trace.OpenRead()))
            {
                reader.ReadLine();  // skip headers

                while (!reader.EndOfStream)
                {
                    string[] line = reader.ReadLine().Split(new string[] { ";\t" }, StringSplitOptions.RemoveEmptyEntries);

                    var cpu = float.Parse(line[3], CultureInfo.InvariantCulture);
                    var memory = float.Parse(line[6], CultureInfo.InvariantCulture);
                    var iops = float.Parse(line[8], CultureInfo.InvariantCulture);
                    var network = float.Parse(line[9], CultureInfo.InvariantCulture);


                    vmTrace.Add(new Resources()
                    {
                        CPU = cpu,
                        Memmory = memory,
                        Network = network,
                        IOPS = iops
                    });
                }
            }

            return vmTrace;
        }


    }
}
