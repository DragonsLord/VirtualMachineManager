using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.Diagnostics.Models
{
    public class DiagnosticParams
    {
        // public int PrognoseDepth { get; set; }

        public ResourceParam<float> Threadhold { get; set; }

        public ResourceParam<float> RecieverThreadhold { get; set; }

        public float CpuOnMigration { get; set; }

        public float MaxNetworkOnMigration { get; set; }
    }
}
