using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Simulation.Models
{
    public class Server
    {
        public int Id { get; set; }

        public Resources Resources { get; private set; }

        public Resources UsedResources {
            get => PrognosedUsedResources[0];
            private set => PrognosedUsedResources[0] = value;
        }

        // 0 index contains real (not predicted) data
        public Resources[] PrognosedUsedResources { get; } = new Resources[GlobalConstants.PROGNOSE_DEPTH + 1];

        public bool TurnedOn { get; private set; }

        public List<VM> RunningVMs { get; private set; } = new List<VM>();

        public void TurnOn()
        {
            if (TurnedOn) {
                return;
            }

            Logger.LogAction($"Server {Id} is turning on");

            TurnedOn = true;
        }

        public void RemoveVM(VM vm)
        {
            RunningVMs.Remove(vm);
            vm.HostServerId = 0;

            vm.OnResourceRequirmentChange -= Vm_OnResourceRequirmentChange;

            UsedResources -= vm.Resources;

            //TODO: resolve PredictedResources (maybe in PrognoseModule ?)
        }

        public void RunVM(VM vm)
        {
            RunningVMs.Add(vm);
            vm.HostServerId = Id;

            for (int depth = 0; depth <= GlobalConstants.PROGNOSE_DEPTH; depth++)
            {
                PrognosedUsedResources[depth] += vm.PrognosedResources[depth];
            }

            vm.OnResourceRequirmentChange += Vm_OnResourceRequirmentChange;
        }

        public bool CanRunVM(VM vm, int depth)
        {
            return Resources - PrognosedUsedResources[depth] > vm.PrognosedResources[depth];
        }
        
        private void Vm_OnResourceRequirmentChange(int depth, Resources diff) => PrognosedUsedResources[depth] += diff;

        public static Server FromDataBaseModel(DAL.Entities.PhysicalMachine pm)
        {
            return new Server()
            {
                Id = pm.Id,
                Resources = new Resources
                {
                    IOPS = pm.IOPS,
                    Memmory = pm.Memmory,
                    CPU = pm.CPU,
                    Network = pm.Network
                }
            };
        }
    }
}
