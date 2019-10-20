using System;
using System.Collections.Generic;
using System.Text;
using VirtualMachineManager.Asigning;
using VirtualMachineManager.DataAccess.Traces.Entities;
using VirtualMachineManager.Services;

namespace VirtualMachineManager.App
{
    public class App
    {
        private readonly IServerManager serverManager;
        private readonly VmAsigner vmAsigner;

        public App(
            IServerManager serverManager,
            IEnumerable<SimualtionTimeEvent> events,
            VmAsigner vmAsigner
            )
        {
            this.serverManager = serverManager;
            this.vmAsigner = vmAsigner;
        }

        public void Start()
        {

        }
    }
}
