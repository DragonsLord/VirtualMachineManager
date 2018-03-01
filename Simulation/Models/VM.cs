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

        public Resources Resources {
            get => PrognosedResources[0];
            private set => PrognosedResources[0] = value;
        }

        // 0 index contains real (not predicted) data
        public Resources[] PrognosedResources { get; } = new Resources[GlobalConstants.PROGNOSE_DEPTH + 1];

        public int HostServerId { get; set; } = 0;

        public event Action<int, Resources> OnResourceRequirmentChange;

        public void UpdateRequirments(DAL.Entities.VMEvent vme)
        {
            var newResources = new Resources
            {
                IOPS = vme.IOPS,
                Memmory = vme.Memmory,
                CPU = vme.CPU,
                Network = vme.Network
            };

            UpdatePrognosedRequirments(0, newResources);
        }

        public void UpdatePrognosedRequirments(int depth, Resources res)
        {
            OnResourceRequirmentChange?.Invoke(depth, res - Resources);

            PrognosedResources[depth] = res;
        }

        // TODO: [debug] remove
        public static int Instances = 0;
        private VM() { Instances++; }

        public static VM FromDataBaseModel(DAL.Entities.VMEvent vme)
        {
            return new VM()
            {
                Id = vme.VMId,
                Resources = new Resources
                {
                    IOPS = vme.IOPS,
                    Memmory = vme.Memmory,
                    CPU = vme.CPU,
                    Network = vme.Network
                }
            };
        }
    }
}
