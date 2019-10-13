using System.Collections.Generic;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.EventManagment.Models;

namespace VirtualMachineManager.EventManagment.EventManager
{
    public class EventManager
    {
        #region Events
        public Event<IEnumerable<VM>> VmsRequested { get; set; }
        public Event<IEnumerable<int>> VmsRemoved { get; set; }

        public Event<IEnumerable<VM>> VmsAsignRejected { get; set; }
        #endregion
    }
}
