using System.Collections.Generic;

namespace VirtualMachineManager.DataAccess.Traces.Entities
{
    public class SimualtionTimeEvent
    {
        public int Id { get; set; }

        public long Time { get; set; }

        public virtual IList<VMEvent> VMEvents { get; set; }

        public virtual IList<RemovedVMEvent> RemovedVM { get; set; }
    }
}
