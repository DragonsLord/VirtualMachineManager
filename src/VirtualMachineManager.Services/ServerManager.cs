using System;
using System.Collections.Generic;
using System.Linq;
using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.Services
{
    public class ServerManager : IServerManager
    {
        private Dictionary<int, Server> servers;
        public ServerManager(IEnumerable<Server> servers)
        {
            this.servers = servers.ToDictionary(s => s.Id);
        }

        public IEnumerable<Server> Servers => servers.Values;

        public Server Get(int serverId) => servers[serverId];

        public void RunVM(Server server, VM vm) => server.AsignVM(vm);

        public void TurnOn(Server server)
        {
        }
    }
}
