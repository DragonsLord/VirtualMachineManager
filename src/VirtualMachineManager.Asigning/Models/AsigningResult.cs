using System.Collections.Generic;
using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.Asigning.Models
{
    public class AsigningResult
    {
        public List<VM> Unasigned { get; set; }
        public List<VM> Asigned { get; set; }
    }
}
