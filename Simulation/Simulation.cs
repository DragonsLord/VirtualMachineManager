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
    public class Simulation
    {
        private PrognoseModule prognoseModule = new PrognoseModule();
        private DiagnosticModule diagnosticModule = new DiagnosticModule();
        private MigrationModule migrationModule = new MigrationModule();
        private AsigningModule asigningModule = new AsigningModule();

        private DataUnit dataContext = new DataUnit();

        public VMCollection VMs { get; private set; } = new VMCollection();
        public ServerCollection Servers { get; private set; }

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

            Logger.RegisterOutputChannels(Console.WriteLine, (s) => System.Diagnostics.Debug.WriteLine(s));
        }

        private void Prepare()
        {
            Servers = new ServerCollection(dataContext.PhysicalMachineRepository);
        }

        public void Run()
        {
            Logger.StartProcessSection("Simulation runnig");

            Prepare();

            #region Main Cycle
            foreach(var timeEvent in dataContext.TimeEventRepository.EnumerateAll())       // ???
            {
                var loggerSectionName = $"Step {timeEvent.Id}";
                Logger.StartProcessSection(loggerSectionName);

                #region Updating resources requirments
                Logger.StartProcessSection("Updating resources requirments");

                HandleRemovedVMs(timeEvent);
                HandleUpdateRequirments(timeEvent);
                var newVMs = GetNewVMs(timeEvent);

                Logger.EndSection("Updating resources requirments");
                #endregion

                // prognosing
                // prognoseModule.Run(VMs);

                // run diagnostic (detect overloaded)
                var overloadedMachines = diagnosticModule.DetectOverloadedMachines(Servers);

                // migrate from overloaded servers
                var migrationPlan = migrationModule.MigrateFromOverloaded(overloadedMachines);

                // assign new VMs
                asigningModule.Asign(newVMs, Servers);
                VMs.AddRange(newVMs);

                // run diagnostic (detect low loaded)
                // var lowloadedMachines = diagnosticModule.DetectLowloadedMachines(Servers);

                // free low loaded servers
                // var releaseMigrationPlan = migrationModule.ReleaseLowloadedMachines(lowloadedMachines.Targets);

                // save migration plan
                if (!migrationPlan.IsEmpty)
                {
                    Logger.LogAction(migrationPlan.GetShortInfo()); // migration dont applies
                }
                Console.WriteLine(VMs.Select(vm => vm.Resources.CPU).Sum());
                Logger.EndSection(loggerSectionName, "finished");
            }
            #endregion

            Logger.EndSection("Simulation", "ended");
            Console.ReadKey();
        }

        private void HandleRemovedVMs(SimualtionTimeEvent timeEvent)
        {
            foreach (var removedVM in timeEvent.RemovedVM)
            {
                var vm = VMs.Get(removedVM.VMId);
                var server = Servers.Get(vm.HostServerId);
                server.RemoveVM(vm);
                VMs.Remove(vm);
            }
        }

        private IEnumerable<VM> GetNewVMs(SimualtionTimeEvent timeEvent)
        {
            /*VMs.AddRange(timeEvent.VMEvents
                                    .Where(vme => vme.IsNew)
                                    .Select(VM.FromDataBaseModel));*/
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
    }
}
