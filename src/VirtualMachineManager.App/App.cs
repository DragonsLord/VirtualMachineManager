using System.Collections.Generic;
using System.Linq;
using VirtualMachineManager.App.Services;
using VirtualMachineManager.Asigning;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.DataAccess.Traces.Entities;
using VirtualMachineManager.Diagnostics;
using VirtualMachineManager.Migration;
using VirtualMachineManager.Services;

namespace VirtualMachineManager.App
{
    public class App
    {
        private readonly IServerManager serverManager;
        private readonly IEnumerable<SimualtionTimeEvent> events;
        private readonly VirtualMachines VMs = new VirtualMachines();

        private readonly VmAsigner vmAsigner;
        private readonly DiagnosticService diagnosticService;
        private readonly MigrationManager migrationManager;

        public App(
            IServerManager serverManager,
            IEnumerable<SimualtionTimeEvent> events,
            VmAsigner vmAsigner,
            DiagnosticService diagnosticService,
            MigrationManager migrationManager
            )
        {
            this.serverManager = serverManager;
            this.events = events;
            this.vmAsigner = vmAsigner;
            this.diagnosticService = diagnosticService;
            this.migrationManager = migrationManager;
        }

        public void Start()
        {
            foreach (var @event in events)
            {
                var newVMs = AdvanceSimulation(@event);

                var overloadedDiagnostic = diagnosticService.DetectOverloadedMachines(serverManager.Servers);

                if (overloadedDiagnostic.Targets.Any())
                {
                    var migrationPlan = migrationManager.MigrateFromOverloaded(
                        overloadedDiagnostic.Targets,
                        overloadedDiagnostic.Recievers);

                    Logger.LogMessage(migrationPlan.GetFullInfo());
                }

                if (newVMs.Count() > 0)
                {
                    // assign new VMs
                    var result = vmAsigner.Asign(newVMs, serverManager.Servers);
                    VMs.Add(result.Asigned);
                }

                var lowloadedDiagnostic = diagnosticService.DetectLowloadedMachines(serverManager.Servers);

                if (lowloadedDiagnostic.Targets.Any())
                {
                    var migrationPlan = migrationManager.ReleaseLowloadedMachines(
                        lowloadedDiagnostic.Targets,
                        lowloadedDiagnostic.Recievers);

                    Logger.LogMessage(migrationPlan.GetFullInfo());
                }
            }
        }

        private IEnumerable<VM> AdvanceSimulation(SimualtionTimeEvent @event)
        {
            HandleRemovedEvent(@event.RemovedVMs);
            VMs.AdvanceRunningVMs(@event.VMEvents);

            return @event.VMEvents.Where(vm => vm.IsNew).Select(Mapper.Map);
        }

        private void HandleRemovedEvent(IList<RemovedVMEvent> removedVms)
        {
            foreach (var removedVM in removedVms)
            {
                var vm = VMs.Get(removedVM.VMId);
                if (vm.HostId > 0)
                {
                    var server = serverManager.Get(vm.HostId);
                    server.RemoveVM(vm);
                }
                VMs.Remove(vm.Id);
            }
        }
    }
}
