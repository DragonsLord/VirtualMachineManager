using System;
using System.Collections.Generic;
using System.Text;
using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.Services
{
    public interface IServerManager
    {
        Server RunVM(Server server, VM vm);

       /* bool IsOverloaded(Server server);

        bool IsLowloaded(Server server);*/

        Server TurnOn(Server server);

        Server Get(int serverId);
    }
}
