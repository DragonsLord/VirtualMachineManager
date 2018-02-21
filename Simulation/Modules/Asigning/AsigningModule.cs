using Simulation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using static Utilities.GlobalConstants;

namespace Simulation.Modules.Asigning
{
    public class AsigningModule
    {
        private List<Server> workingServers;
        private List<Server> disabledServers;

        public void Asign(IEnumerable<VM> vms, IEnumerable<Server> servers)
        {
            workingServers = servers.Where(s => s.TurnedOn).ToList();
            disabledServers = servers.Where(s => !s.TurnedOn).ToList();

            FirstFitDecreasing(vms);
        }

        private void FirstFitDecreasing(IEnumerable<VM> vms)
        {
            Logger.StartProcessSection("Assigning VMs");
            bool unAsigned = false;
            foreach (var vm in vms.OrderByDescending(GetVMResourceVolume))
            {
                unAsigned = true;
                foreach (var server in workingServers)
                {
                    if (server.CanRunVM(vm, s => s.UsedResources))
                    {
                        server.RunVM(vm);
                        unAsigned = false;
                        break;
                    }
                }
                if (unAsigned)
                {
                    foreach (var server in disabledServers)
                    {
                        if (server.CanRunVM(vm, s => s.UsedResources))
                        {
                            TurnOnServer(server);
                            server.RunVM(vm);
                            break;
                        }
                    }
                }
            }
            Logger.EndSection("Assigning VMs");
        }

        private void TurnOnServer(Server server)
        {
            server.TurnOn();
            workingServers.Add(server);
            disabledServers.Remove(server);
        }

        private float GetVMResourceVolume(VM vm)
        {
            // TODO: [heuristic] improve
            var res = vm.Resources;
            return
                (CPU_WEIGHT * res.CPU + IOPS_WEIGHT * res.IOPS +
                 MEMMORY_WEIGHT * res.Memmory + NETWORK_WEIGHT * res.Network)
                / (CPU_WEIGHT + IOPS_WEIGHT + MEMMORY_WEIGHT + NETWORK_WEIGHT);
        }
    }
}
