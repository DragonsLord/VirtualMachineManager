using System.Collections.Generic;
using System.Linq;
using VirtualMachineManager.App.Services;
using VirtualMachineManager.Asigning;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.DataAccess.Traces.Entities;
using VirtualMachineManager.Diagnostics;
using VirtualMachineManager.Migration;
using VirtualMachineManager.Migration.Model;
using VirtualMachineManager.Prognosing;
using VirtualMachineManager.Services;

namespace VirtualMachineManager.App
{
    public class App
    {
        private readonly ServerCollection servers;
        private readonly IReportService reportService;
        private readonly IEnumerable<SimualtionTimeEvent> events;
        private readonly VirtualMachines VMs = new VirtualMachines();
        private readonly MigrationJobs migrations = new MigrationJobs();

        private readonly VmAsigner vmAsigner;
        private readonly DiagnosticService diagnosticService;
        private readonly MigrationManager migrationManager;
        private readonly PrognosingService prognosingService;

        public App(
            IEnumerable<SimualtionTimeEvent> events,
            IReportService reportService,
            ServerCollection serverCollection,
            VmAsigner vmAsigner,
            DiagnosticService diagnosticService,
            MigrationManager migrationManager,
            PrognosingService prognosingService
            )
        {
            this.servers = serverCollection;
            this.reportService = reportService;
            this.events = events;
            this.vmAsigner = vmAsigner;
            this.diagnosticService = diagnosticService;
            this.migrationManager = migrationManager;
            this.prognosingService = prognosingService;
        }

        public void Start()
        {
            reportService.Initialize(servers.Select(s => s.Id));

            foreach (var @event in events)
            {
                var newVMs = AdvanceSimulation(@event);

                var overloadedDiagnostic = diagnosticService.DetectOverloadedMachines(servers);

                if (overloadedDiagnostic.Targets.Any())
                {
                    var migrationPlan = migrationManager.MigrateFromOverloaded(
                        overloadedDiagnostic.Targets,
                        overloadedDiagnostic.Recievers);

                    Logger.LogMessage(migrationPlan.GetFullInfo());

                    ApplyMigrations(migrationPlan);
                }

                if (newVMs.Count() > 0)
                {
                    // assign new VMs
                    var result = vmAsigner.Asign(newVMs, servers);
                    VMs.Add(result.Asigned);
                }

                var lowloadedDiagnostic = diagnosticService.DetectLowloadedMachines(servers);

                if (lowloadedDiagnostic.Targets.Any())
                {
                    var migrationPlan = migrationManager.ReleaseLowloadedMachines(
                        lowloadedDiagnostic.Targets,
                        lowloadedDiagnostic.Recievers);

                    Logger.LogMessage(migrationPlan.GetFullInfo());

                    ApplyMigrations(migrationPlan);
                }

                foreach (var server in servers) reportService.WriteServerStatistics(@event.Id, server);
            }

            reportService.DrawCharts();
            reportService.Save();
        }

        private void ApplyMigrations(MigrationPlan migrationPlan)
        {
            if (migrationPlan.IsEmpty) return;

            foreach (var migration in migrationPlan)
            {
                migrations.Add(new MigrationTask(
                        migration.Target,
                        servers.Get(migration.SourceId),
                        servers.Get(migration.RecieverId)));
            }
        }

        private IEnumerable<VM> AdvanceSimulation(SimualtionTimeEvent @event)
        {
            HandleRemovedEvent(@event.RemovedVMs);
            VMs.AdvanceRunningVMs(@event.VMEvents);
            migrations.Advance();

            return @event.VMEvents.Where(vm => vm.IsNew).Select(Mapper.Map);
        }

        private void HandleRemovedEvent(IList<RemovedVMEvent> removedVms)
        {
            foreach (var removedVM in removedVms)
            {
                var vm = VMs.Get(removedVM.VMId);
                if (vm.HostId > 0)
                {
                    var server = servers.Get(vm.HostId);
                    server.RemoveVM(vm);
                }
                VMs.Remove(vm.Id);
            }
        }
    }
}
