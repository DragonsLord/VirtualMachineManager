
namespace DAL.Entities
{
    public class PhysicalMachine
    {
        public int Id { get; set; }

        public float IOPS { get; set; }

        public float Memmory { get; set; }

        public float CPU { get; set; }

        public float Network { get; set; }
    }
}
