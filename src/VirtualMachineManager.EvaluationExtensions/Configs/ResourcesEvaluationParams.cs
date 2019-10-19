using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.EvaluationExtensions.Configs
{
    public class ResourcesEvaluationParams
    {
        public ResourceParam<float> Weight { get; set; }
        public ResourceParam<float> Cap { get; set; }
    }
}
