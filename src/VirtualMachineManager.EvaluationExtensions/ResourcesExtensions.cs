using VirtualMachineManager.Core.Models;
using VirtualMachineManager.EvaluationExtensions.Configs;

namespace VirtualMachineManager.EvaluationExtensions
{
    public static class ResourcesExtensions
    {
        public static ResourcesEvaluationParams Config { get; } = new ResourcesEvaluationParams();

        public static float GetValue(this Resources res) =>
                Config.Weight.CPU * res.CPU / Config.Cap.CPU +
                Config.Weight.IOPS * res.IOPS / Config.Cap.IOPS +
                Config.Weight.Memmory * res.Memmory / Config.Cap.Memmory +
                Config.Weight.Network * res.Network / Config.Cap.Network;
    }
}
