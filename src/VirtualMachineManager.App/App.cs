using System;
using System.Collections.Generic;
using System.Linq;
using VirtualMachineManager.App.Services;
using VirtualMachineManager.Asigning;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.DataAccess.Traces.Entities;
using VirtualMachineManager.Diagnostics;
using VirtualMachineManager.Diagnostics.Models;
using VirtualMachineManager.Migration;
using VirtualMachineManager.Migration.Model;
using VirtualMachineManager.Prognosing;
using VirtualMachineManager.Prognosing.Models;
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

        private ServerCollection[] prognoseStates;

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

            prognoseStates = new ServerCollection[prognosingService.Params.PrognoseDepth];
        }

        public void Start()
        {
            reportService.Initialize(servers.Select(s => s.Id));

            foreach (var @event in events)
            {
                Logger.LogMessage($"Simualtion step {@event.Id} (t={@event.Time})");
                var newVMs = AdvanceSimulation(@event);

                Prognose(@event.Id);

                DiagnoseAndMigrateIfNeeded(diagnosticService.DetectOverloadedMachines, migrationManager.MigrateFromOverloaded);

                if (newVMs.Count() > 0)
                {
                    // assign new VMs
                    var result = vmAsigner.Asign(newVMs, servers);
                    VMs.Add(result.Asigned);
                }

                // TODO: should we used prgnoses for releasing too?
                DiagnoseAndMigrateIfNeeded(diagnosticService.DetectLowloadedMachines, migrationManager.ReleaseLowloadedMachines);

                WriteServerStatistics(@event.Id);
            }

            SaveResults();
        }

        public void SaveResults()
        {
            reportService.DrawCharts();
            reportService.Save();
        }

        private void Prognose(int eventId)
        {
            if (prognosingService.CanPrognose(eventId))
            {
                var prognoses =  prognosingService.Forecast(VMs, eventId);
                for (int i = 0; i < prognosingService.Params.PrognoseDepth; i++)
                {
                    prognoseStates[i] = MakePrognosedState(prognoses, i + 1);
                }
            }
        }

        private void DiagnoseAndMigrateIfNeeded(
            Func<IEnumerable<Server>, DiagnosticResult> diagnose,
            Func<IEnumerable<Server>, IEnumerable<Server>, MigrationPlan> migrate
            )
        {
            for(int i = 0; i <= prognosingService.Params.PrognoseDepth; i++)
            {
                var serversState = i == 0 ? servers : prognoseStates[i-1];

                if (serversState == null) return;

                var overloadedDiagnostic = diagnose(serversState);

                if (overloadedDiagnostic.Targets.Any())
                {
                    var migrationPlan = migrate(
                        overloadedDiagnostic.Targets,
                        overloadedDiagnostic.Recievers);

                    ApplyMigrations(migrationPlan);
                    return;
                }
            };
        }

        private ServerCollection MakePrognosedState(IEnumerable<VMPrognose> prognoses, int prognoseDepth)
        {
            var prognosedState = servers.GetCopies();

            foreach (var vmPrognose in prognoses)
            {
                var serverId = vmPrognose.VM.HostId;
                prognosedState.Get(serverId).AsignVM(vmPrognose.GetPrognosedVmState(prognoseDepth));
            }

            return prognosedState;
        }

        private void ApplyMigrations(MigrationPlan migrationPlan)
        {
            if (migrationPlan.IsEmpty) return;

            Logger.LogMessage(migrationPlan.GetFullInfo());

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
            prognosingService.UpdateTraces(VMs, @event.Id);

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

        private void WriteServerStatistics(int eventId)
        {
            foreach (var server in servers)
            {
                var prognosedResources = prognoseStates.Select(sc => sc?.Get(server.Id)?.UsedResources ?? Resources.Empty);
                reportService.WriteServerStatistics(eventId, server, prognosedResources);
            }
        }
    }
}
