using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Simulation.Models
{
    public class VM
    {
        public int Id { get; set; }

        public Resources Resources { get; set; }
        
        public int HostServerId { get; set; } = 0;

        public bool IsMigrating { get; set; } = false;

        public event Action<Resources> OnResourceRequirmentChange;

        public void UpdateRequirments(DAL.Entities.VMEvent vme)
        {
            var newResources = new Resources
            {
                IOPS = vme.IOPS,
                Memmory = vme.Memory,
                CPU = vme.CPU,
                Network = vme.Network
            };
            
            OnResourceRequirmentChange?.Invoke(newResources - Resources);

            Resources = newResources;
        }

        public static VM FromDataBaseModel(DAL.Entities.VMEvent vme)
        {
            return new VM()
            {
                Id = vme.VMId,
                Resources = new Resources
                {
                    IOPS = vme.IOPS,
                    Memmory = vme.Memory,
                    CPU = vme.CPU,
                    Network = vme.Network
                }
            };
        }
    }
}
