﻿using System;
using System.IO;
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
            string logsFolder = "Logs";
            string inputFolder = args.Any() ? args[0] : "Input";
            string settingsPath = "Settings.ini";

            var identifier = DateTime.Now.ToString("yyyy-MM-dd hh mm ss");
            var logFileName = $"{logsFolder}\\Simualtion log - {identifier}.txt";
            var reportPath = $"{outputFolder}\\Servers statistics - {identifier}.xlsx";

            //using (var streamWriter = new StreamWriter(File.Create(logFileName)))
            {
                using(var appBuilder = new AppBuilder())
                {
                    var app = appBuilder
                        .SetupDirectory(dataFolder)
                        .SetupDirectory(outputFolder)
                        .SetupDirectory(logsFolder)
                        .WithSettingsFrom(settingsPath)
                        .WithLoggerOutputs(Console.Write/*, streamWriter.Write*/)
                        .WithTracesDataContext(
                            new TracesDataContextBuilder()
                                .WithDbFilePath($"{dataFolder}\\traces.db")
                                .WithInputTracesPath(inputFolder)
                                .Build())
                        .OutputTo(reportPath)
                        .Build();

                    app.Start();
                }
                
            }
        }
    }
}