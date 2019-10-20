
namespace VirtualMachineManager.DataAccess.Traces.Entities
{
    public class VMEvent
    {
        public int VMId { get; set; }

        public bool IsNew { get; set; }

        public int TimeEventId { get; set; }

        public float IOPS { get; set; }

        public float Memory { get; set; }

        public float CPU { get; set; }

        public int CpuCores { get; set; }

        public float Network { get; set; }
    }
}
