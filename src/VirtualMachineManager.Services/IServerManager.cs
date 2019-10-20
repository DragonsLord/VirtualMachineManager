using System;
using System.Collections.Generic;
using System.Text;
using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.Services
{
    public interface IServerManager
    {
        void RunVM(Server server, VM vm);

        void TurnOn(Server server);

        Server Get(int serverId);

        IEnumerable<Server> Servers { get; }
    }
}
