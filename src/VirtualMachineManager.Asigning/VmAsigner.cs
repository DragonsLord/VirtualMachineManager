using System;
using System.Collections.Generic;
using System.Linq;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.Asigning.Models;
using VirtualMachineManager.Services;
using VirtualMachineManager.EvaluationExtensions;

namespace VirtualMachineManager.Asigning
{
    public class VmAsigner
    {
        private readonly AsigningParams _params;

        public VmAsigner(AsigningParams settings)
        {
            _params = settings;
        }

        public AsigningResult Asign(IEnumerable<VM> vms, IEnumerable<Server> servers)
        {
            var workingServers = servers
                .Where(s => s.TurnedOn)
                .OrderBy(s => s.ResourcesCapacity.GetValue()) // TODO: Is it worth it?
                .ToList();
            var disabledServers = servers
                .Where(s => !s.TurnedOn)
                .OrderBy(s => s.ResourcesCapacity.GetValue())
                .ToList();
            
            return BestFitDecreasing(vms, workingServers, disabledServers);
        }

        private void FirstFitDecreasing(
            IEnumerable<VM> vms,
            IList<Server> workingServers,
            IList<Server> disabledServers)
        {
            Logger.StartProcess("Assigning VMs");
            bool unAsigned = false;
            foreach (var vm in vms.OrderByDescending(vm => vm.Resources.GetValue()))
            {
                unAsigned = true;
                foreach (var server in workingServers)
                {
                    if (server.CanRunVM(vm) /*server.CanRunVM(vm, 0)*/)
                    {
                        server.AsignVM(vm);
                        unAsigned = false;
                        break;
                    }
                }
                if (unAsigned)
                {
                    foreach (var server in disabledServers)
                    {
                        if (server.CanRunVM(vm) /*server.CanRunVM(vm, 0)*/)
                        {
                            TurnOnServer(server, workingServers, disabledServers);
                            server.AsignVM(vm);
                            break;
                        }
                    }
                }
            }
            Logger.EndProccess("Assigning VMs");
        }

        private AsigningResult BestFitDecreasing(
            IEnumerable<VM> vms,
            IList<Server> workingServers,
            IList<Server> disabledServers)
        {
            float getDiff(Server s, Resources r) {
                Resources res = s.UsedResources + r;
                var desired = new Resources()
                {
                    CPU = s.ResourcesCapacity.CPU * _params.DesiredLoadLevel.CPU,
                    Network = s.ResourcesCapacity.CPU * _params.DesiredLoadLevel.Network,
                    Memmory = s.ResourcesCapacity.CPU * _params.DesiredLoadLevel.Memmory,
                    IOPS = s.ResourcesCapacity.CPU * _params.DesiredLoadLevel.IOPS
                };
                return Math.Abs((res - desired).GetValue());
            }
            Logger.StartProcess("Assigning VMs");
            var rejected = new List<VM>();
            var added = new List<VM>();
            bool unAsigned = false;
            float minVolume = float.PositiveInfinity;
            Server currentServer = null;
            foreach (var vm in vms.OrderByDescending(vm => vm.Resources.GetValue()))
            {
                unAsigned = true;
                minVolume = float.PositiveInfinity;
                foreach (var server in workingServers)
                {
                    if (server.CanRunVM(vm) /*server.CanRunVM(vm, 0)*/)
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
                        if (server.CanRunVM(vm) /*server.CanRunVM(vm, 0)*/)
                        {
                            TurnOnServer(server, workingServers, disabledServers);
                            currentServer = server;
                            break;
                        }
                    }
                }
                if (currentServer != null)
                {
                    currentServer.AsignVM(vm);
                    currentServer = null;
                    added.Add(vm);
                } else if (vm.HostId == 0)
                {
                    rejected.Add(vm);
                    Logger.LogMessage($"VM{vm.Id} was rejected");
                }
            }
            Logger.EndProccess("Assigning VMs");
            return new AsigningResult()
            {
                Unasigned = rejected,
                Asigned = added
            };
        }

        private void TurnOnServer(Server server, IList<Server> working, IList<Server> disabled)
        {
            server.TurnOn();
            working.Add(server);
            disabled.Remove(server);
        }
    }
}
