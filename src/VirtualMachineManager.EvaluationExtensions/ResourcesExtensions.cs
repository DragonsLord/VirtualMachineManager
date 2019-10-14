using VirtualMachineManager.Core.Models;
using VirtualMachineManager.EvaluationExtensions.Configs;

namespace VirtualMachineManager.EvaluationExtensions
{
    public static class ResourcesExtensions
    {
        public static ResourcesEvaluationParams Config { get; } = new ResourcesEvaluationParams();

        public static float GetValue(this Resources res)
        {
            return
                (Config.CpuWeight * res.CPU / Config.CpuCap +
                Config.IopsWeight * res.IOPS / Config.IopsCap +
                Config.MemoryWeight * res.Memmory / Config.MemoryCap +
                Config.NetworkWeight * res.Network / Config.NetworkCap);
        }
    }
}
