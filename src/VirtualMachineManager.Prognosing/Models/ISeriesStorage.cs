using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.Prognosing.Models
{
    public interface ISeriesStorage
    {
        public Task PushNextRecord(IEnumerable<VM> vms, long timestamp);

        public Task<IEnumerable<Resources>> GetVMTrace(int vmId);
    }
}
