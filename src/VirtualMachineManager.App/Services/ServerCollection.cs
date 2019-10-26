using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.App.Services
{
    public class ServerCollection: IEnumerable<Server>
    {
        private Dictionary<int, Server> servers;
        public ServerCollection(IEnumerable<Server> servers)
        {
            this.servers = servers.ToDictionary(s => s.Id);
        }

        public Server Get(int serverId) => servers[serverId];

        public ServerCollection GetCopies() => new ServerCollection(servers.Values.Select(s => s.CopyWithoutVms()));

        public IEnumerator<Server> GetEnumerator() => servers.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => servers.Values.GetEnumerator();
    }
}
