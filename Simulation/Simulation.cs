using DAL;
using DAL.Entities;
using Simulation.Models;
using Simulation.Models.Collections;
using Simulation.Modules.Asigning;
using Simulation.Modules.Diagnostic;
using Simulation.Modules.Migration;
using Simulation.Modules.Prognosing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Simulation
{
    //TODO: add detection of unhandled server overloading (cause system to freeze);
    public class Simulation
    {
        private string _logFileName;
        private string _serverStatisticsFolder;
        private ExcelWrapper _excel;

        private PrognoseModule prognoseModule = new PrognoseModule();
        private DiagnosticModule diagnosticModule = new DiagnosticModule();
        private MigrationModule migrationModule = new MigrationModule();
        private AsigningModule asigningModule = new AsigningModule();

        private DataUnit dataContext = new DataUnit();

        public VMCollection VMs { get; private set; } = new VMCollection();
        public ServerCollection Servers { get; private set; }

        public event Action<Simulation> OnNextStep;

        static Simulation()
        {
            #region register App Variables
            var appDataDir = Environment.CurrentDirectory + "\\AppData";
            if (!Directory.Exists(appDataDir))
            {
                Directory.CreateDirectory(appDataDir);
            }
            AppDomain.CurrentDomain.SetData("DataDirectory", appDataDir);
            #endregion

            Logger.RegisterOutputChannels(Console.Write);
        }

        private void Prepare()
        {
            Servers = new ServerCollection(dataContext.PhysicalMachineRepository);
            Servers.ForEach((server) => this.OnNextStep += server.TryShutdown);
            var identifier = DateTime.Now.ToString("yyyy-MM-dd hh mm ss");
            _logFileName = $"Logs\\Simualtion log - {identifier}.txt";
            _excel = new ExcelWrapper($"Logs\\Servers statistics - {identifier}.xlsx");
            _excel.Initialize(Servers.Count);
        }
        
        public void Run()
        {
            Prepare();

            using (var streamWriter = new StreamWriter(File.Create(_logFileName)))
            {
                Logger.RegisterOutputChannels(streamWriter.Write);
                Logger.StartProcess("Simulation runnig");
                #region Main Cycle
                foreach (var timeEvent in dataContext.TimeEventRepository.EnumerateAll().Take(200))
                {
                    OnNextStep?.Invoke(this);
                    ProccessEvent(timeEvent);
                    LogCurrentServerState(timeEvent.Id);
                }
                #endregion

                Logger.EndProccess("Simulation", "ended");
                _excel.DrawCharts();
                _excel.Save();
                _excel.Dispose();
            }
            Console.ReadKey();
        }

        private void ProccessEvent(SimualtionTimeEvent timeEvent)
        {
            var loggerSectionName = $"Step {timeEvent.Id}";
            Logger.StartProcess(loggerSectionName);

            #region Updating resources requirments
            Logger.StartProcess("Updating resources requirments");

            HandleRemovedVMs(timeEvent);
            HandleUpdateRequirments(timeEvent);
            var newVMs = GetNewVMs(timeEvent);
            Logger.LogMessage($"New VMs: {newVMs.Count}");

            Logger.EndProccess("Updating resources requirments");
            #endregion

            // prognosing
            prognoseModule.Run(Servers);

            // run diagnostic (detect overloaded)
            var overloadedDiagnosticResult = diagnosticModule.DetectOverloadedMachines(Servers);

            if (overloadedDiagnosticResult.AreTargetMachines)
            {
                // migrate from overloaded servers
                var migrationPlan = migrationModule.MigrateFromOverloaded(overloadedDiagnosticResult);
                if (!migrationPlan.IsEmpty)
                {
                    ApplyMigrations(migrationPlan);
                }
            }
            if (newVMs.Count > 0)
            {
                // assign new VMs
                asigningModule.Asign(newVMs, Servers);
                VMs.AddRange(newVMs);
            }

            // run diagnostic (detect low loaded)
            var lowloadedDiagnosticPlan = diagnosticModule.DetectLowloadedMachines(Servers);

            if (lowloadedDiagnosticPlan.AreTargetMachines)
            {
                // free low loaded servers
                var releaseMigrationPlan = migrationModule.ReleaseLowloadedMachines(lowloadedDiagnosticPlan);
                if (!releaseMigrationPlan.IsEmpty)
                {
                    ApplyMigrations(releaseMigrationPlan);
                }
            }

            // Console.WriteLine(VMs.Select(vm => vm.Resources.CPU).Sum());
            Logger.EndProccess(loggerSectionName, "finished");
        }

        private void HandleRemovedVMs(SimualtionTimeEvent timeEvent)
        {
            Logger.LogMessage($"{timeEvent.RemovedVM.Count} VMs is finished");
            foreach (var removedVM in timeEvent.RemovedVM)
            {
                var vm = VMs.Get(removedVM.VMId);
                if (vm.HostServerId > 0)
                {
                    var server = Servers.Get(vm.HostServerId);
                    server.RemoveVM(vm);
                }
                VMs.Remove(vm);
            }
        }

        private List<VM> GetNewVMs(SimualtionTimeEvent timeEvent)
        {
            return timeEvent.VMEvents
                                    .Where(vme => vme.IsNew)
                                    .Select(VM.FromDataBaseModel)
                                    .ToList();
        }

        private void HandleUpdateRequirments(SimualtionTimeEvent timeEvent)
        {
            VMs.Update(timeEvent.VMEvents.Where(vme => !vme.IsNew));
        }

        private IEnumerable<Server> GetRunningServers() => Servers.Where(server => server.TurnedOn);

        private void ApplyMigrations(MigrationPlan migrationPlan)
        {
            Logger.LogMessage(migrationPlan.GetShortInfo());
            foreach (var migrationTask in migrationModule.ApplayMigrations(migrationPlan, Servers))
            {
                OnNextStep += migrationTask.OnNextTimeEvent;
            }
            Logger.LogMessage(migrationPlan.GetFullInfo());
        }

        private void LogCurrentServerState(int step)
        {
            foreach (var server in Servers)
            {
                _excel.WriteServerStatistics(step, server);
            }
        }
    }
}
