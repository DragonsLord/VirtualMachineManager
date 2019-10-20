using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using VirtualMachineManager.DataAccess.Traces.Entities;
using VirtualMachineManager.Services;

namespace VirtualMachineManager.DataAccess.Traces
{
    public class TracesParser
    {
        private readonly string inputDataFolderPath;
        private readonly string VMTracesFolder = "Traces";
        private readonly string serversFile = "servers.csv";
        private readonly string timeRangeFile = "TimeRange.txt";

        private readonly TracesDataContext dataContext;
        private long startTime = 0;
        private long timeStep = 100;

        public TracesParser(TracesDataContext dataContext, string tracesFolderPath = "InputData")
        {
            this.dataContext = dataContext;
            inputDataFolderPath = tracesFolderPath;
        }

        public void ParseData()
        {
            Logger.StartProcess("Mapping Traces Data to DataBase Model");

            CreateTimeEvents();
            MapPhysicalMachines();
            MapFromVMTraces();

            Logger.EndProccess("Mapping Traces Data to DataBase Model");
        }

        private int GetTimeEventId(long time) => (int)((time - startTime) / timeStep) + 1;

        private void MapFromVMTraces()
        {
            var tracesDir = new DirectoryInfo(Path.Combine(inputDataFolderPath, VMTracesFolder));
            var removedVmEvents = new List<RemovedVMEvent>();

            Logger.StartProcess("Reading traces");

            foreach (var trace in tracesDir.GetFiles())
            {
                var vmId = int.Parse(trace.Name.Substring(0, trace.Name.Length - trace.Extension.Length));

                var vmTrace = ReadTrace(trace, vmId, removedVmEvents);

                dataContext.BulkInsert(vmTrace);

                Logger.LogMessage($"VM {vmId} - done");
            }

            Logger.EndProccess("Reading treaces", "done");
            dataContext.BulkInsert(removedVmEvents);
        }

        private IEnumerable<VMEvent> ReadTrace(FileInfo trace, int vmId, IList<RemovedVMEvent> removed)
        {
            var vmEvents = new List<VMEvent>();

            bool isNew = true;
            using (StreamReader reader = new StreamReader(trace.OpenRead()))
            {
                reader.ReadLine();  // skip headers

                while (!reader.EndOfStream)
                {
                    string[] line = reader.ReadLine().Split(new String[] { ";\t" }, StringSplitOptions.RemoveEmptyEntries);

                    var time = long.Parse(line[0]);
                    var cpuCores = int.Parse(line[1], CultureInfo.InvariantCulture);
                    var cpu = float.Parse(line[3], CultureInfo.InvariantCulture);
                    var memmory = float.Parse(line[6], CultureInfo.InvariantCulture);
                    var iops = float.Parse(line[8], CultureInfo.InvariantCulture);
                    var network = float.Parse(line[9], CultureInfo.InvariantCulture);

                    if (cpu == 0 && memmory == 0 && iops == 0 && network == 0)
                    {
                        if (!isNew)
                        {
                            removed.Add(new RemovedVMEvent()
                            {
                                TimeEventId = GetTimeEventId(time),
                                VMId = vmId
                            });
                        }
                        isNew = true;
                        continue;
                    }

                    var vmEvent = new VMEvent()
                    {
                        TimeEventId = GetTimeEventId(time),
                        CPU = cpu,
                        CpuCores = cpuCores,
                        Memory = memmory,
                        IOPS = iops,
                        Network = network,
                        VMId = vmId,
                        IsNew = isNew
                    };

                    // check for duplicate record by timeId
                    if (vmEvents.LastOrDefault()?.TimeEventId != vmEvent.TimeEventId)
                        vmEvents.Add(vmEvent);

                    isNew = false;
                }
            }

            return vmEvents;
        }

        private void CreateTimeEvents()
        {
            var strData = File.ReadAllText(Path.Combine(inputDataFolderPath, timeRangeFile)).Split(';');
            startTime = long.Parse(strData[0]);
            timeStep = long.Parse(strData[1]);
            var endTime = long.Parse(strData[2]);
            int count = (int)((endTime - startTime) / timeStep) + 1;
            var timeEvents = new SimualtionTimeEvent[count];
            for (var i = 0; i < count; i++)
            {
                timeEvents[i] = new SimualtionTimeEvent
                {
                    Id = i + 1,
                    Time = startTime + timeStep * i
                };
            }

            dataContext.BulkInsert(timeEvents);
        }

        private void MapPhysicalMachines()
        {
            Logger.StartAction("Reading PM capacities");
            using (StreamReader reader = new StreamReader(Path.Combine(inputDataFolderPath, serversFile)))
            {
                reader.ReadLine();  // skip headers

                while (!reader.EndOfStream)
                {
                    string[] line = reader.ReadLine().Split(new String[] { ";\t" }, StringSplitOptions.RemoveEmptyEntries);

                    var amount = int.Parse(line[2]);
                    var cpuFrequency = float.Parse(line[3], CultureInfo.InvariantCulture); // Mhz
                    var cpuCores = int.Parse(line[5], CultureInfo.InvariantCulture);
                    var memmory = float.Parse(line[6], CultureInfo.InvariantCulture); // GB
                    var iops = float.Parse(line[7], CultureInfo.InvariantCulture);  //Mb/s
                    var network = float.Parse(line[8], CultureInfo.InvariantCulture);   // Gbit/s

                    for (int i = 0; i < amount; i++)
                    {
                        dataContext.PhysicalMachines.Add(new PhysicalMachine()
                        {
                            CPU = cpuFrequency * cpuCores,
                            Memory = memmory * 1024 * 1024, // kb
                            IOPS = iops * 1024,  // kb/s
                            Network = network * 1024 * 1024 // kbit/s
                        });
                    }
                }
            }
            dataContext.SaveChanges();
            Logger.EndAction();
        }
    }
}
