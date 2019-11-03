using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using VirtualMachineManager.App.Services;
using VirtualMachineManager.Asigning;
using VirtualMachineManager.DataAccess.Traces;
using VirtualMachineManager.Diagnostics;
using VirtualMachineManager.EvaluationExtensions;
using VirtualMachineManager.Migration;
using VirtualMachineManager.Prognosing;
using VirtualMachineManager.Prognosing.Algorythms;
using VirtualMachineManager.Prognosing.Models;
using VirtualMachineManager.Services;
using VirtualMachineManager.TimeSeriesStorage.Influx;

namespace VirtualMachineManager.App
{
    public class AppBuilder: IDisposable
    {
        private ParametersManager parametersManager;
        private TracesDataContext tracesDataContext;
        private ISeriesStorage seriesStorage;
        private string outputFolder;

        private List<Action> disposableResources = new List<Action>();

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
            disposableResources.Add(dataContext.Dispose);
            return this;
        }

        public AppBuilder WithLocalInfluxDb(string dbExePath, string dbName)
        {
            var startInfo = new ProcessStartInfo(dbExePath)
            {
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            var p = Process.Start(startInfo);
            disposableResources.Add(() =>
            {
                p.Kill();
                p.Dispose();
            });

            var influxDbUrl = "http://localhost:8086/";
            var influxStorage = new InfluxStorage(new InfluxHttpClient(influxDbUrl), dbName);
            disposableResources.Add(influxStorage.Dispose);
            seriesStorage = influxStorage;
            return this;
        }

        public AppBuilder WithREngine(string packagesPath)
        {
            RGlobalEnvironment.InitREngineWithForecasing(packagesPath);
            disposableResources.Add(RGlobalEnvironment.R.Dispose);
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

            var reportService = new ReportService(outputFolder, parametersManager.PrognoseDepth);
            disposableResources.Add(reportService.Dispose);

            return new App(
                events,
                reportService,
                new ServerCollection(tracesDataContext.PhysicalMachines.AsEnumerable().Select(Mapper.Map)),
                new VmAsigner(parametersManager.GetAsigningParams()),
                new DiagnosticService(parametersManager.GetDiagnosticParams()),
                new MigrationManager(parametersManager.GetMigrationParams()),
                new PrognosingService(parametersManager.GetPrognosingParams(), seriesStorage));
        }

        private void SetupEvaluationConfigs()
        {
            ResourcesExtensions.Config = parametersManager.GetResourcesEvaluationParams();
            ServerExtensions.Config = parametersManager.GetServerEvaluationParams();
        }

        public void Dispose()
        {
            foreach (var disposeResource in disposableResources)
            {
                disposeResource();
            }
        }
    }
}
