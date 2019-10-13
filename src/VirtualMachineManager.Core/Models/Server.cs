using System;
using System.Collections.Generic;
using System.Linq;

namespace VirtualMachineManager.Core.Models
{
    public class Server
    {
        public int Id { get; set; }

        public Resources ResourcesCapacity { get; set; }

        // TODO: [idea] do not store all prognose values and just used the biggest value from prognosing (there should not be any prognose values in this model)
        public Resources UsedResources { get; set; }

        public bool TurnedOn { get; set; }

        public List<VM> RunningVMs { get; set; } = new List<VM>();
    }
}
