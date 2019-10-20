using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.DataAccess.Traces.Entities;

namespace VirtualMachineManager.App.Services
{
    public class VirtualMachines
    {
        private Dictionary<int, VM> _vms = new Dictionary<int, VM>();

        public VM Get(int vmId) => _vms[vmId];

        public void Remove(int vmId) => _vms.Remove(vmId);

        public void Add(IEnumerable<VM> vms)
        {
            foreach (var vm in vms)
            {
                _vms.Add(vm.Id, vm);
            }
        }

        public void AdvanceRunningVMs(IEnumerable<VMEvent> vmEvents)
        {
            foreach (var vmEvent in vmEvents.Where(vm => !vm.IsNew))
            {
                _vms[vmEvent.VMId].Resources = new Resources
                {
                    CPU = vmEvent.CPU,
                    Network = vmEvent.Network,
                    Memmory = vmEvent.Memory,
                    IOPS = vmEvent.IOPS
                };
            }
        }
    }
}
