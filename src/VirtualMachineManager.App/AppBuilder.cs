using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VirtualMachineManager.App.Services;
using VirtualMachineManager.Asigning;
using VirtualMachineManager.DataAccess.Traces;
using VirtualMachineManager.Diagnostics;
using VirtualMachineManager.EvaluationExtensions;
using VirtualMachineManager.Migration;
using VirtualMachineManager.Services;

namespace VirtualMachineManager.App
{
    public class AppBuilder: IDisposable
    {
        private ParametersManager parametersManager;
        private TracesDataContext tracesDataContext;
        private string outputFolder;

        private List<IDisposable> disposableResources = new List<IDisposable>();

        public AppBuilder SetupDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return this;
        }

        public AppBuilder WithSettingsFrom(string filePath)
        {
            parametersManager = new ParametersManager(filePath);
            return this;
        }

        public AppBuilder WithTracesDataContext(TracesDataContext dataContext)
        {
            tracesDataContext = dataContext;
            disposableResources.Add(dataContext);
            return this;
        }

        public AppBuilder WithLoggerOutputs(params Action<string>[] channels)
        {
            Logger.RegisterOutputChannels(channels);
            return this;
        }

        public AppBuilder OutputTo(string folderPath)
        {
            outputFolder = folderPath;
            return this;
        }

        public App Build()
        {
            SetupEvaluationConfigs();

            var events = Enumerable
                .Range(0, parametersManager.StepsToSimulate ?? tracesDataContext.TimeEvents.Count())
                .Select(i => tracesDataContext.TimeEvents.Include(te => te.VMEvents).Include(te => te.RemovedVMs).Skip(i).First());

            var reportService = new ReportService(outputFolder);
            disposableResources.Add(reportService);

            return new App(
                events,
                reportService,
                new ServerCollection(tracesDataContext.PhysicalMachines.AsEnumerable().Select(Mapper.Map)),
                new VmAsigner(parametersManager.GetAsigningParams()),
                new DiagnosticService(parametersManager.GetDiagnosticParams()),
                new MigrationManager(parametersManager.GetMigrationParams()));
        }

        private void SetupEvaluationConfigs()
        {
            ResourcesExtensions.Config = parametersManager.GetResourcesEvaluationParams();
            ServerExtensions.Config = parametersManager.GetServerEvaluationParams();
        }

        public void Dispose()
        {
            foreach (var resource in disposableResources)
            {
                resource.Dispose();
            }
        }
    }
}
