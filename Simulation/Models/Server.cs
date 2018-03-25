using Simulation.Modules.Diagnostic;
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
        public Resources[] PrognosedUsedResources { get; private set; } = new Resources[GlobalConstants.PROGNOSE_DEPTH + 1];

        public bool TurnedOn { get; private set; }

        public bool InMigration { get; set; }   //TODO: to aviod migration plan repeating

        public List<VM> RunningVMs { get; private set; } = new List<VM>();

        public void TurnOn()
        {
            if (TurnedOn) {
                return;
            }

            Logger.LogMessage($"Server {Id} is turning on");

            TurnedOn = true;
        }

        public void MarkToShutdown()
        {
            TurnedOn = false;
        }

        public void RemoveVM(VM vm)
        {
            RunningVMs.Remove(vm);
            vm.HostServerId = 0;

            vm.OnResourceRequirmentChange -= Vm_OnResourceRequirmentChange;

            for (int i = 0; i <= GlobalConstants.PROGNOSE_DEPTH; i++)
            {
                PrognosedUsedResources[i] -= vm.PrognosedResources[i];
            }
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
            var required = PrognosedUsedResources[depth] + vm.Resources;
            return !Evaluator.IsOverloaded(required, this);
        }

        public Server ShalowCopy()
        {
            // TODO: check if work properly
            return new Server()
            {
                Id = this.Id,
                Resources = this.Resources,
                TurnedOn = this.TurnedOn,
                PrognosedUsedResources = this.PrognosedUsedResources.ToArray(),
                RunningVMs = this.RunningVMs.ToList()
            };
        }

        public void TryShutdown(Simulation simulation)
        {
            if (TurnedOn && !RunningVMs.Any())
            {
                TurnedOn = false;
                Logger.LogMessage($"Server {Id} is shutting down");
            }
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
                    Memmory = pm.Memory,
                    CPU = pm.CPU,
                    Network = pm.Network
                }
            };
        }
    }
}
