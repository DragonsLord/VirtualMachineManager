using RDotNet;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            var inputList = new GenericVector(R, traces.Select(ConvertToRList));
            R.SetSymbol(InputListName, inputList);
            timer.Start();
            var result = R.Evaluate($"foreach(i = {InputListName}) %dopar% {ProccessTraceFunction}(i, {Params.MinTraceWindow}, {windowsCount})");

            timer.Stop();
            Debug.WriteLine($"{traces.Count()} with {windowsCount} windows evaluated in {timer.ElapsedMilliseconds}ms");

            return result.AsList().Select(r => ReadRList(r.AsList()));
        }

        private void InitRScript()
        {
            R.Evaluate($"horizon = {Params.PrognoseDepth}");
            R.Evaluate(
                ProccessTraceFunction + @"<- function(trace, windowSize, windows) {
                    #arimaResult = foreach(i=1:windows) %do% {
                    #    arimaFit = auto.arima(window(trace$series, i, windowSize - 1 + i), lambda=0, biasadj=TRUE)
                    #    list(offset=i, result=forecast(arimaFit, h = horizon, level = 95)$mean)
                    #}
                    sesResult = foreach(i=1:windows) %do%
                        list(offset=i, result=ses(window(trace$series, i, windowSize - 1 + i), h=horizon)$mean)

                    holtResult = foreach(i=1:windows) %do%
                        list(offset=i, result=holt(window(trace$series, i, windowSize - 1 + i), h=horizon)$mean)

                    list(vmId = trace$vmId, resourceId = trace$resourceId, ses = sesResult, holt = holtResult)
                }"
            );
        }

        private GenericVector ConvertToRList(VmResourceTrace trace)
        {
            var list = new GenericVector(R, 3);
            list.SetNames("vmId", "resourceId", "series");
            list["vmId"] = new IntegerVector(R, new int[] { trace.VmId });
            list["resourceId"] = new IntegerVector(R, new int[] { (int)trace.Resource });
            list["series"] = new NumericVector(R, trace.Series.Select(f => (double)(f + 1)));
            return list;
        }

        private VmResourceForecast ReadRList(GenericVector row)
        {
            return new VmResourceForecast(
                row["vmId"].AsInteger()[0],
                (Resource)row["resourceId"].AsInteger()[0],
                new Dictionary<string, IEnumerable<ForecastResult>>
                    {
                        // { "ARIMA", ParseForecastResult(row["arima"].AsList())},
                        { "SES", ParseForecastResult(row["ses"].AsList())},
                        { "HOLT", ParseForecastResult(row["holt"].AsList())}
                    }
                );
        }

        private IEnumerable<ForecastResult> ParseForecastResult(GenericVector list) =>
            list.Select(exp => exp.AsList())
                .Select(r => new ForecastResult(
                    r["offset"].AsInteger()[0],
                    r["result"].AsNumeric().Select(f => (float)(f - 1)).ToArray()));
    }
}
