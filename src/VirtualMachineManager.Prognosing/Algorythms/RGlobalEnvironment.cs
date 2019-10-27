using RDotNet;
using System.Collections.Generic;

namespace VirtualMachineManager.Prognosing.Algorythms
{
    public static class RGlobalEnvironment
    {
        public const string TimeSeries = "ts";
        public const string RealValue = "realValue";
        public static REngine R { get; private set; }

        public static void InitREngineWithForecasing(string packageLibPath)
        {
            var engine = REngine.GetInstance();

            // Add package lib path
            engine.Evaluate($".libPaths(c('{packageLibPath}', .libPaths()))");

            // install (if not installed) and load forecast package
            string script = $"if (!require('forecast')) install.packages('forecast', repos='https://cloud.r-project.org/')";

            engine.Evaluate(script);

            R = engine;
        }

        public static void SetTimeSeries(IEnumerable<double> series) =>
            R.SetSymbol(TimeSeries, R.CreateNumericVector(series));

        public static void SetRealValue(double val) =>
            R.SetSymbol(RealValue, R.CreateNumericVector(new double[] { val }));

    }
}
