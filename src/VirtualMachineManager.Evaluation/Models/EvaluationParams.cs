using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.Evaluation.Models
{
    public class EvaluationParams
    {
        public ResourceParam<float> Weight { get; set; }

        public ResourceParam<float> Cap { get; set; }

        public ResourceParam<float> Threadhold { get; set; }

        public ResourceParam<float> RecieverThreadhold { get; set; }

        public ResourceParam<float> UnderloadThreadhold { get; set; }

        public float CpuOnMigration { get; set; }

        public float MaxNetworkOnMigration { get; set; }
    }
}
