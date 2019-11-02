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

            var inputList = new GenericVector(R, traces.Select(ConvertToRList));
            R.SetSymbol(InputListName, inputList);
            timer.Start();
            var result = R.Evaluate($"foreach(i = {InputListName}) %dopar% {ProccessTraceFunction}(i)");

            timer.Stop();
            Debug.WriteLine($"{traces.Count()}  evaluated in {timer.ElapsedMilliseconds}ms");

            return result.AsList().Select(r => ReadRList(r.AsList()));
        }

        private void InitRScript()
        {
            R.Evaluate($"prognoseHorizon = {Params.PrognoseDepth}");
            R.Evaluate(
                ProccessTraceFunction + @"<- function(trace) {
                    ARIMAfit = auto.arima(trace$series, lambda=0, biasadj=TRUE)
                    arimaResult = forecast(ARIMAfit, h = prognoseHorizon, level = 95)
                    sesResult <- ses(trace$series, h=prognoseHorizon)
                    holtResult <- holt(trace$series, h=prognoseHorizon)
                    list(vmId = trace$vmId, resourceId = trace$resourceId, arima = arimaResult$mean, ses = sesResult$mean, holt = holtResult$mean)
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
                new Dictionary<string, float[]>
                    {
                        { "ARIMA", row["arima"].AsNumeric().Select(f => (float)(f - 1)).ToArray() },
                        { "SES", row["ses"].AsNumeric().Select(f => (float)(f - 1)).ToArray() },
                        { "HOLT", row["holt"].AsNumeric().Select(f => (float)(f - 1)).ToArray() }
                    }
                );
        }
    }
}
