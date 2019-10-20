﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualMachineManager.DataAccess.Traces.Entities
{
    public class RemovedVMEvent
    {
        public int VMId { get; set; }

        public int TimeEventId { get; set; }

        public virtual SimualtionTimeEvent TimeEvent { get; set; }
    }
}
