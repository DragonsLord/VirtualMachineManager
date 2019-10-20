using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualMachineManager.App.Services;
using VirtualMachineManager.Asigning;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.DataAccess.Traces.Entities;
using VirtualMachineManager.Services;

namespace VirtualMachineManager.App
{
    public class App
    {
        private readonly IServerManager serverManager;
        private readonly IEnumerable<SimualtionTimeEvent> events;
        private readonly VirtualMachines VMs = new VirtualMachines();

        private readonly VmAsigner vmAsigner;

        public App(
            IServerManager serverManager,
            IEnumerable<SimualtionTimeEvent> events,
            VmAsigner vmAsigner
            )
        {
            this.serverManager = serverManager;
            this.events = events;
            this.vmAsigner = vmAsigner;
        }

        public void Start()
        {
            foreach (var @event in events)
            {
                var newVMs = AdvanceSimulation(@event);

                if (newVMs.Count() > 0)
                {
                    // assign new VMs
                    var result = vmAsigner.Asign(newVMs, serverManager.Servers);
                    VMs.Add(result.Asigned);
                }
            }
        }

        private IEnumerable<VM> AdvanceSimulation(SimualtionTimeEvent @event)
        {
            HandleRemovedEvent(@event.RemovedVMs);
            VMs.AdvanceRunningVMs(@event.VMEvents);

            return @event.VMEvents.Where(vm => vm.IsNew).Select(Mapper.Map);
        }

        private void HandleRemovedEvent(IList<RemovedVMEvent> removedVms)
        {
            foreach (var removedVM in removedVms)
            {
                var vm = VMs.Get(removedVM.VMId);
                if (vm.HostId > 0)
                {
                    var server = serverManager.Get(vm.HostId);
                    server.RemoveVM(vm);
                }
                VMs.Remove(vm.Id);
            }
        }
    }
}
