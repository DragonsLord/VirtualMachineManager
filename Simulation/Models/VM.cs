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

        public Resources Resources { get; private set; }

        public Resources[] PredictedResources { get; } = new Resources[GlobalConstants.PROGNOSE_DEPTH];

        public int HostServerId { get; set; } = 0;

        public event Action<Resources> OnResourceRequirmentChange;
        public event Action<int, Resources> OnPredictedResourceRequirmentChange;

        public void UpdateRequirments(DAL.Entities.VMEvent vme)
        {
            var newResources = new Resources
            {
                IOPS = vme.IOPS,
                Memmory = vme.Memmory,
                CPU = vme.CPU,
                Network = vme.Network
            };

            OnResourceRequirmentChange(newResources - Resources);

            Resources = newResources;
        }

        public void UpdatePredictedRequirments(int depth, Resources res)
        {
            OnPredictedResourceRequirmentChange(depth, res - Resources);

            PredictedResources[depth] = res;
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
