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

        public Server Get(int serverId)
        {
            throw new NotImplementedException();
        }

        public Server RunVM(Server server, VM vm)
        {
            throw new NotImplementedException();
        }

        public Server TurnOn(Server server)
        {
            throw new NotImplementedException();
        }
    }
}
