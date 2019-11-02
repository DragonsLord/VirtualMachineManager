using System.Collections.Generic;

namespace VirtualMachineManager.Prognosing.Algorythms
{
    public enum Resource
    {
        Cpu = 1,
        Memory = 2,
        Network = 3,
        Iops = 4
    }

    public class VmResourceTrace
    {
        public int VmId { get; private set; }

        public Resource Resource { get; private set; }

        public IEnumerable<float> Series { get; private set; }

        public VmResourceTrace(int vmId, Resource resource, IEnumerable<float> series)
        {
            VmId = vmId;
            Resource = resource;
            Series = series;
        }
    }

    public class ForecastResult
    {
        public int WindowOffset { get; private set; }
        public float[] Result { get; private set; }

        public ForecastResult(int windowOffset, float[] forecasts)
        {
            WindowOffset = windowOffset;
            Result = forecasts;
        }
    }

    public class VmResourceForecast
    {
        public int VmId { get; private set; }

        public Resource Resource { get; private set; }

        public Dictionary<string, IEnumerable<ForecastResult>> Forecast { get; private set; }

        public VmResourceForecast(int vmId, Resource resource, Dictionary<string, IEnumerable<ForecastResult>> forecast)
        {
            VmId = vmId;
            Resource = resource;
            Forecast = forecast;
        }
    }
}
