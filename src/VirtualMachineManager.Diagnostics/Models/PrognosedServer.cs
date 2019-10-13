using System;
using System.Collections.Generic;
using System.Text;
using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.Diagnostics.Models
{
    public class PrognosedServer
    {
        public Server Server { get; }

        public Resources PrognosedUsage { get; } // TODO: [think] store all or one value ?

        public int Id => Server.Id;

        public Resources CurrentUsage => Server.UsedResources;

        public Resources AllResources => Server.ResourcesCapacity;

        public PrognosedServer(Server server, Resources prognose)
        {
            Server = server;
            PrognosedUsage = prognose;
        }
    }
}
