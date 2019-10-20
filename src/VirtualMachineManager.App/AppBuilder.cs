using System;
using System.IO;
using VirtualMachineManager.App.Services;
using VirtualMachineManager.Services;

namespace VirtualMachineManager.App
{
    public class AppBuilder
    {
        private ParametersManager parametersManager;
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

        public AppBuilder WithLoggerOutputs(params Action<string>[] channels)
        {
            Logger.RegisterOutputChannels(channels);
            return this;
        }
    }
}
