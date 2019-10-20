﻿using System;
using System.IO;
using VirtualMachineManager.DataAccess.Traces;

namespace VirtualMachineManager.App
{
    class Program
    {
        static void Main()
        {
            string dataFolder = "Data";
            string outputFolder = "Result";
            string inputFolder = "Input";
            string settingsPath = "Settings.ini";

            var identifier = DateTime.Now.ToString("yyyy-MM-dd hh mm ss");
            var logFileName = $"{outputFolder}\\Simualtion log - {identifier}.txt";

            using (var streamWriter = new StreamWriter(File.Create(logFileName)))
            {
                using (var tracesContext = new TracesDataContextBuilder()
                    .WithDbFilePath($"{dataFolder}\\traces.db")
                    .WithInputTracesPath(inputFolder)
                    .Build())
                {
                    new AppBuilder()
                        .SetupDirectory(dataFolder)
                        .SetupDirectory(outputFolder)
                        .WithSettingsFrom(settingsPath)
                        .WithLoggerOutputs(Console.Write, streamWriter.Write);
                }
                
            }
        }
    }
}
