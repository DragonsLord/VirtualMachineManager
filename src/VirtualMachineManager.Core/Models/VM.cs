using System;

namespace VirtualMachineManager.Core.Models
{
    public class VM
    {
        public int Id { get; set; }

        public bool IsMigrating { get; set; } // Remove ?

        public Resources Resources { get; set; }
        
        public int HostId { get; set; } = 0;
    }
}
