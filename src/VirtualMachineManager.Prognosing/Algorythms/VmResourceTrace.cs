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

    public class VmResourceForecast
    {
        public int VmId { get; private set; }

        public Resource Resource { get; private set; }

        public float[] Forecast { get; private set; }

        public VmResourceForecast(int vmId, Resource resource, float[] forecast)
        {
            VmId = vmId;
            Resource = resource;
            Forecast = forecast;
        }
    }
}
