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
            workingServers = servers
                .Where(s => s.TurnedOn)
                .OrderBy(s => s.Resources.EvaluateVolume()) // TODO: Is it worth it?
                .ToList();
            disabledServers = servers
                .Where(s => !s.TurnedOn)
                .OrderBy(s => s.Resources.EvaluateVolume())
                .ToList();
            
            BestFitDecreasing(vms);
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

        private void BestFitDecreasing(IEnumerable<VM> vms)
        {
            float getDiff(Server s, Resources r) {
                Resources res = s.UsedResources + r;
                var desired = new Resources()
                {
                    CPU = s.Resources.CPU * CPU_DESIRED_LEVEL,
                    Network = s.Resources.CPU * NETWORK_DESIRED_LEVEL,
                    Memmory = s.Resources.CPU * MEMMORY_DESIRED_LEVEL,
                    IOPS = s.Resources.CPU * IOPS_DESIRED_LEVEL
                };
                return Math.Abs((res - desired).EvaluateVolume());
            }
            Logger.StartProcess("Assigning VMs");
            bool unAsigned = false;
            float minVolume = float.PositiveInfinity;
            Server currentServer = null;
            foreach (var vm in vms.OrderByDescending(GetVMResourceVolume))
            {
                unAsigned = true;
                minVolume = float.PositiveInfinity;
                foreach (var server in workingServers)
                {
                    if (server.CanRunVM(vm, 0))
                    {
                        var diff = getDiff(server, vm.Resources);
                        if (diff < minVolume)
                        {
                            currentServer = server;
                            minVolume = diff; 
                        }
                        unAsigned = false;
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
                if (currentServer != null)
                {
                    currentServer.RunVM(vm);
                    currentServer = null;
                } else
                {
                    Logger.LogMessage($"VM{vm.Id} was rejected");
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
            var res = vm.Resources;
            return res.EvaluateVolume();
        }
    }
}
