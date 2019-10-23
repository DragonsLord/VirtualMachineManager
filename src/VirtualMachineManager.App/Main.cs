using System;
using System.Linq;
using VirtualMachineManager.DataAccess.Traces;

namespace VirtualMachineManager.App
{
    class Program
    {
        static void Main(string[] args)
        {
            string dataFolder = "Data";
            string outputFolder = "Result";
            string inputFolder = args.Any() ? args[0] : "Input";
            string settingsPath = "Settings.ini";

            // var identifier = DateTime.Now.ToString("yyyy-MM-dd hh mm ss");
            // var logFileName = $"{outputFolder}\\Simualtion log - {identifier}.txt";

            // using (var streamWriter = new StreamWriter(File.Create(logFileName)))
            {
                using(var appBuilder = new AppBuilder())
                {
                    var app = appBuilder
                        .SetupDirectory(dataFolder)
                        .SetupDirectory(outputFolder)
                        .WithSettingsFrom(settingsPath)
                        .WithLoggerOutputs(Console.Write)
                        .WithTracesDataContext(
                            new TracesDataContextBuilder()
                                .WithDbFilePath($"{dataFolder}\\traces.db")
                                .WithInputTracesPath(inputFolder)
                                .Build())
                        .Build();

                    app.Start();
                }
                
            }
        }
    }
}
