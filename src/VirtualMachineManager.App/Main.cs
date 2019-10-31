using System;
using System.IO;
using System.Linq;
using VirtualMachineManager.DataAccess.Traces;

namespace VirtualMachineManager.App
{
    class Program
    {
        static void Main(string[] args)
        {
            string inputFolder = args.Any() ? args[0] : "Input";
            string dataFolder = args.Any() ? Path.Combine(args[0], "Data") : "Data";
            string outputFolder = "Result";
            string logsFolder = "Logs";
            string settingsPath = "Settings.ini";
            string influxdPath = @"D:\Projects\VirtualMachineManager\influxdb-1.7.8-1\influxd.exe";
            string rPackagesPath = "rPackages";

            string identifier = DateTime.Now.ToString("yyyy-MM-dd hh mm ss");
            string logFileName = $"{logsFolder}\\Simualtion log - {identifier}.txt";
            string reportPath = $"{outputFolder}\\Servers statistics - {identifier}.xlsx";

            //using (var streamWriter = new StreamWriter(File.Create(logFileName)))
            using(var appBuilder = new AppBuilder())
            {
                var app = appBuilder
                    .SetupDirectory(dataFolder)
                    .SetupDirectory(outputFolder)
                    .SetupDirectory(logsFolder)
                    .SetupDirectory(rPackagesPath)
                    .WithSettingsFrom(settingsPath)
                    .WithLoggerOutputs(Console.Write/*, streamWriter.Write*/)
                    .WithTracesDataContext(
                        new TracesDataContextBuilder()
                            .WithDbFilePath($"{dataFolder}\\traces.db")
                            .WithInputTracesPath(inputFolder)
                            .Build())
                    .WithLocalInfluxDb(influxdPath, "vm_traces")
                    .WithREngine(rPackagesPath)
                    .OutputTo(reportPath)
                    .Build();
                try
                {
                    app.Start();
                }catch (Exception ex) {
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Sumilation stopped due to:");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                    Console.ForegroundColor = color;
                }
            }
        }
    }
}
