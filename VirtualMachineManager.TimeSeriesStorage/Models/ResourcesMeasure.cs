using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.TimeSeriesStorage.Models
{
    public class ResourcesMeasure
    {
        public Resources Resources { get; }
        public string Name { get; }
        public long Timestamp { get; }

        public ResourcesMeasure(string name, long time, Resources resources)
        {
            Name = name;
            Timestamp = time;
            Resources = resources;
        }
    }
}
