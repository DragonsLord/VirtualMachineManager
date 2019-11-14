using RDotNet;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.Prognosing.Models;

namespace VirtualMachineManager.Prognosing.Algorythms
{
    public class RForecast
    {
        private const string ProccessTraceFunction = "proccess_trace";
        private const string InputListName = "traces";
        private readonly PrognosingParams Params;

        public REngine R => RGlobalEnvironment.R;

        public RForecast(PrognosingParams @params)
        {
            Params = @params;
            InitRScript();
        }

        public IEnumerable<VmResourceForecast> RunAlgorythms(IEnumerable<VmResourceTrace> traces)
        {
            var timer = new Stopwatch();

            var windowsCount = traces.First().Series.Count() - Params.MinTraceWindow + 1;
            var splitedTraces = traces.GroupBy(t => t.Series.Count(v => v > 0) >= 2).ToDictionary(p => p.Key ? 1 : 0, p => p.ToArray());

            var zeroResult = Enumerable.Range(0, windowsCount).Select(i => new ForecastResult(i, Enumerable.Repeat<float>(0, Params.PrognoseDepth).ToArray()));
            var zeroSeriesResult = splitedTraces[0]
                .Select(s => new VmResourceForecast(s.VmId, s.Resource, new Dictionary<string, IEnumerable<ForecastResult>>()
                {
                        { "ARIMA", zeroResult},
                        { "SES", zeroResult},
                        { "CROST", zeroResult},
                        { "HOLT", zeroResult},
                        { "DHOLT", zeroResult}
                }));

            var inputList = new GenericVector(R, splitedTraces[1].Select(ConvertToRList));
            R.SetSymbol(InputListName, inputList);

            timer.Start();

            var result = R.Evaluate($"foreach(i = {InputListName}) %dopar% {ProccessTraceFunction}(i, {Params.MinTraceWindow}, {windowsCount})");

            timer.Stop();
            Debug.WriteLine($"{traces.Count()} with {windowsCount} windows evaluated in {timer.ElapsedMilliseconds}ms");

            return result.AsList().Select(r => ReadRList(r.AsList())).Concat(zeroSeriesResult);
        }

        private void InitRScript()
        {
            R.Evaluate($"horizon = {Params.PrognoseDepth}");
            R.Evaluate(
                ProccessTraceFunction + @"<- function(trace, windowSize, windows) {
                    arimaResult = foreach(i=1:windows) %do% {
                        ar = tryCatch({
                            forecast(auto.arima(window(trace$series, i, windowSize - 1 + i), lambda=0, biasadj=TRUE), h = horizon, level = 95)$mean
                        },
                        error=function(cond) {
                            return(NA)
                        })
                        list(offset=i-1, result=ar)
                    }
                    sesResult = foreach(i=1:windows) %do%
                        list(offset=i-1, result=ses(window(trace$series, i, windowSize - 1 + i), h=horizon)$mean)

                    crostResult = foreach(i=1:windows) %do% {
                        cr = tryCatch({
                            crost(window(trace$series, i, windowSize - 1 + i), h=horizon)$frc.out
                        },
                        error=function(cond) {
                            return(NA)
                        })
                        list(offset=i-1, result=cr)
                    }
                        

                    holtResult = foreach(i=1:windows) %do%
                        list(offset=i-1, result=holt(window(trace$series, i, windowSize - 1 + i), h=horizon)$mean)

                    dholtResult = foreach(i=1:windows) %do%
                        list(offset=i-1, result=holt(window(trace$series, i, windowSize - 1 + i), damped=TRUE, phi = 0.9, h=horizon)$mean)

                    list(vmId = trace$vmId, resourceId = trace$resourceId, ses = sesResult, arima=arimaResult, crost = crostResult, holt = holtResult, dholt = dholtResult)
                }"
            );
        }

        private GenericVector ConvertToRList(VmResourceTrace trace)
        {
            var list = new GenericVector(R, 3);
            list.SetNames("vmId", "resourceId", "series");
            list["vmId"] = new IntegerVector(R, new int[] { trace.VmId });
            list["resourceId"] = new IntegerVector(R, new int[] { (int)trace.Resource });
            list["series"] = new NumericVector(R, trace.Series.Select(f => (double)(f)));
            return list;
        }

        private VmResourceForecast ReadRList(GenericVector row)
        {
            var forecasts = new[] { "arima", "ses", "crost", "holt", "dholt" }
                .ToDictionary(x => x, method => ParseForecastResult(row[method].AsList()))
                .Where(p => p.Value.Any())
                .ToDictionary(p => p.Key.ToUpper(), p => p.Value);

            return new VmResourceForecast(
                row["vmId"].AsInteger()[0],
                (Resource)row["resourceId"].AsInteger()[0],
                forecasts);
        }

        private IEnumerable<ForecastResult> ParseForecastResult(GenericVector list) =>
            list.Select(exp => exp.AsList())
                .Select(r => {
                    var offset = r["offset"].AsInteger()[0];
                    var r_reslult = r["result"].AsNumeric();
                    float[] result = double.IsNaN(r_reslult[0]) ? null : r_reslult.Select(f => (float)f).ToArray();
                    return new ForecastResult(offset, result);
                }).Where(x => x.Result != null);
    }
}
