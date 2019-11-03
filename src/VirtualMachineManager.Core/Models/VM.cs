using System;

namespace VirtualMachineManager.Core.Models
{
    public class VM
    {
        public int Id { get; set; }

        public bool IsMigrating { get; set; } // Remove ?

        public Resources Resources { get; set; }
        
        public int HostId { get; private set; } = 0;

        public void AsignToHost(int hostId)
        {
            HostId = hostId;
        }

        public void Terminate()
        {
            HostId = 0;
        }
    }
}
