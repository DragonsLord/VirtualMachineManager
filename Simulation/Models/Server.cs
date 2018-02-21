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

        public Resources UsedResources { get; private set; }

        public Resources[] PredictedUsedResources { get; } = new Resources[GlobalConstants.PROGNOSE_DEPTH];

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
            vm.OnPredictedResourceRequirmentChange -= Vm_OnPredictedResourceRequirmentChange;

            UsedResources -= vm.Resources;

            //TODO: resolve PredictedResources (maybe in PrognoseModule ?)
        }

        public void RunVM(VM vm)
        {
            // TODO: add resource constrains
            RunningVMs.Add(vm);
            vm.HostServerId = Id;

            vm.OnResourceRequirmentChange += Vm_OnResourceRequirmentChange;
            vm.OnPredictedResourceRequirmentChange += Vm_OnPredictedResourceRequirmentChange;
        }

        public bool CanRunVM(VM vm, Func<Server, Resources> selector)
        {
            // TODO: deside if selector for VM is needed
            return Resources - selector(this) > vm.Resources;
        }

        private void Vm_OnResourceRequirmentChange(Resources diff) => UsedResources += diff;

        private void Vm_OnPredictedResourceRequirmentChange(int depth, Resources diff) => PredictedUsedResources[depth] += diff;

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
