using RDotNet;
using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualMachineManager.Prognosing.Algorythms
{
    public class RForecast
    {
        private const string ArimaResultVariable = "ArimaPrediction";
        private readonly int prognoseHorizon;

        private bool arimaResultExist = false;

        public REngine R => RGlobalEnvironment.R;

        public RForecast(int prognoseHorizon)
        {
            this.prognoseHorizon = prognoseHorizon;
        }

        public Dictionary<string, double[]> RunAlgorythms(string traceId, IEnumerable<double> series)
        {
            RGlobalEnvironment.SetTimeSeries(series);

            return new Dictionary<string, double[]>
            {
                { AlgorythmNames.ARIMA, GetArimaResults(traceId) }
            };
        }

        public Dictionary<string, double> GetMAPE(string traceId, double realValue)
        {
            RGlobalEnvironment.SetRealValue(realValue);

            return new Dictionary<string, double>
            {
                { AlgorythmNames.ARIMA, GetArimaAccuracy(traceId) }
            };
        }

        private double[] GetArimaResults(string traceId)
        {
            R.Evaluate($"ARIMAfit = auto.arima({RGlobalEnvironment.TimeSeries}, lambda=0, biasadj=TRUE)");
            return R.Evaluate($"{ArimaResultVariable}{traceId} = forecast(ARIMAfit, h = {prognoseHorizon}, level = 95)")
                .AsList()[3].AsNumeric().ToArray();
        }

        private double GetArimaAccuracy(string traceId)
        {
            if (!arimaResultExist)
            {
                return 0;
            }
            return R.Evaluate($"accuracy({ArimaResultVariable}{traceId}, {RGlobalEnvironment.RealValue})").AsNumeric()[9];
        }
    }
}
