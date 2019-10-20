
namespace VirtualMachineManager.DataAccess.Traces.Entities
{
    public class PhysicalMachine
    {
        public int Id { get; set; }

        public float IOPS { get; set; }

        public float Memory { get; set; }

        public float CPU { get; set; }

        public float Network { get; set; }
    }
}
