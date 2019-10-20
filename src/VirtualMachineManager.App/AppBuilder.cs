using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using VirtualMachineManager.App.Services;
using VirtualMachineManager.Asigning;
using VirtualMachineManager.DataAccess.Traces;
using VirtualMachineManager.EvaluationExtensions;
using VirtualMachineManager.Services;

namespace VirtualMachineManager.App
{
    public class AppBuilder: IDisposable
    {
        private ParametersManager parametersManager;
        private TracesDataContext tracesDataContext;

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
            return this;
        }

        public AppBuilder WithLoggerOutputs(params Action<string>[] channels)
        {
            Logger.RegisterOutputChannels(channels);
            return this;
        }

        public App Build()
        {
            SetupEvaluationConfigs();

            var serverManager = new ServerManager(tracesDataContext.PhysicalMachines.AsEnumerable().Select(Mapper.Map));

            var events = Enumerable
                .Range(0, parametersManager.StepsToSimulate ?? tracesDataContext.TimeEvents.Count())
                .Select(i => tracesDataContext.TimeEvents.Include(te => te.VMEvents).Include(te => te.RemovedVMs).Skip(i).First());

            return new App(serverManager, events, new VmAsigner(parametersManager.GetAsigningParams(), serverManager));
        }

        private void SetupEvaluationConfigs()
        {
            ResourcesExtensions.Config = parametersManager.GetResourcesEvaluationParams();
            ServerExtensions.Config = parametersManager.GetServerEvaluationParams();
        }

        public void Dispose()
        {
            tracesDataContext?.Dispose();
        }
    }
}
