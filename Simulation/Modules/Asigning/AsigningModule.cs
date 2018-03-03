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
            Logger.StartProcess("Assigning VMs");
            bool unAsigned = false;
            foreach (var vm in vms.OrderByDescending(GetVMResourceVolume))
            {
                unAsigned = true;
                foreach (var server in workingServers)
                {
                    if (server.CanRunVM(vm, 0))
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
                        if (server.CanRunVM(vm, 0))
                        {
                            TurnOnServer(server);
                            server.RunVM(vm);
                            break;
                        }
                    }
                }
            }
            Logger.EndProccess("Assigning VMs");
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
            return res.EvaluateVolume();
        }
    }
}
