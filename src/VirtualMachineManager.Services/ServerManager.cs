using System;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.Services.Models;

namespace VirtualMachineManager.Services
{
    public class ServerManager : IServerManager
    {
        private ServerMgmtParams _params;

        public ServerManager(ServerMgmtParams setting) // TODO: insert EventManager
        {
            _params = setting;
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
