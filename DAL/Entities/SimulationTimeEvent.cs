using System.Collections.Generic;

namespace DAL.Entities
{
    public class SimualtionTimeEvent
    {
        public int Id { get; set; }

        public long Time { get; set; }

        public virtual IList<VMEvent> VMEvents { get; set; }

        public virtual IList<RemovedVMEvent> RemovedVM { get; set; }
    }
}
