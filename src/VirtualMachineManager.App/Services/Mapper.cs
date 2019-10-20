using VirtualMachineManager.Core.Models;
using VirtualMachineManager.DataAccess.Traces.Entities;

namespace VirtualMachineManager.App.Services
{
    public static class Mapper
    {
        public static Server Map(PhysicalMachine pm) =>
            new Server()
            {
                Id = pm.Id,
                ResourcesCapacity = new Resources
                {
                    IOPS = pm.IOPS,
                    Memmory = pm.Memory,
                    CPU = pm.CPU,
                    Network = pm.Network
                }
            };
    }
}
