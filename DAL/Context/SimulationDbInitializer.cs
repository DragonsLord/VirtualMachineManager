using System;
using System.Collections.Generic;
using System.Data.Entity;
using DAL.Entities;
using Utilities;
using System.IO;
using System.Globalization;
using EntityFramework.BulkInsert.Extensions;

namespace DAL.Context
{
    public class SimulationDbInitializer: DropCreateDatabaseIfModelChanges<SimulationContext>
    {
        private readonly string inputDataFolder = "InputData";
        private readonly string VMTracesFolder = "Traces";
        private readonly string serversFile = "servers.csv";
        private readonly string timeRangeFile = "TimeRange.txt";
        private long startTime = 0;
        private long timeStep = 100;
        private List<RemovedVMEvent> removedVMEvents = new List<RemovedVMEvent>(GlobalConstants.VM_CAPACITY);
        
        private int GetTimeEventId(long time)
        {
            return (int)((time - startTime) / timeStep) + 1;
        }

        private void MapFromVMTraces(SimulationContext context)
        {
            var tracesDir = new DirectoryInfo(Path.Combine(inputDataFolder, VMTracesFolder));
            int vmId = 0;

            Logger.StartProcess("Reading traces");

            foreach(var trace in tracesDir.GetFiles())
            {
                vmId = int.Parse(trace.PureName());

                var vmTrace = ReadTrace(trace, vmId);

                context.BulkInsert(vmTrace);

                Logger.LogMessage($"VM {vmId} - done");
            }

            Logger.EndProccess("Reading treaces", "done");
        }

        private IEnumerable<VMEvent> ReadTrace(FileInfo trace, int vmId)
        {
            var vmEvents = new List<VMEvent>(GlobalConstants.TIME_STEPS_CAPACITY);

            bool isNew = true;
            using(StreamReader reader = new StreamReader(trace.OpenRead()))
            {
                reader.ReadLine();  // skip headers

                while (!reader.EndOfStream)
                {
                    string[] line = reader.ReadLine().Split(new String[] { ";\t"}, StringSplitOptions.RemoveEmptyEntries);

                    var time = long.Parse(line[0]);
                    var cpu = float.Parse(line[3], CultureInfo.InvariantCulture);
                    var memmory = float.Parse(line[6], CultureInfo.InvariantCulture) / 1024; // to Mbytes
                    var iops = float.Parse(line[8], CultureInfo.InvariantCulture);
                    var network = float.Parse(line[9], CultureInfo.InvariantCulture);

                    if (cpu == 0 && memmory == 0 && iops == 0 && network == 0)
                    {
                        if (!isNew)
                        {
                            CreateRemovedVMEvent(vmId, time);
                        }
                        isNew = true;
                        continue;
                    }
                    
                    vmEvents.Add(new VMEvent()
                    {
                        TimeEventId = GetTimeEventId(time),
                        CPU = cpu,
                        Memory = memmory,
                        IOPS = iops,
                        Network = network,
                        VMId = vmId,
                        IsNew = isNew
                    });

                    isNew = false;
                }
            }

            return vmEvents;
        }

        private void CreateRemovedVMEvent(int vmId, long time)
        {
            removedVMEvents.Add(new RemovedVMEvent()
            {
                TimeEventId = GetTimeEventId(time),
                VMId = vmId
            });
        }

        private void CreateTimeEvents(SimulationContext context)
        {
            var strData = File.ReadAllText(Path.Combine(inputDataFolder, timeRangeFile)).Split(';');
            startTime = long.Parse(strData[0]);
            timeStep = long.Parse(strData[1]);
            var endTime = long.Parse(strData[2]);
            int count = (int)((endTime - startTime) / timeStep);
            var timeEvents = new SimualtionTimeEvent[count];
            for(var i = 0; i < count; i++)
            {
                timeEvents[i] = new SimualtionTimeEvent
                {
                    Id = i + 1,
                    Time = startTime + timeStep * i
                };
            }
            context.BulkInsert(timeEvents);
        }

        private void MapPhysicalMachines(SimulationContext context)
        {
            Logger.StartAction("Reading PM capacities");
            using (StreamReader reader = new StreamReader(Path.Combine(inputDataFolder, serversFile)))
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
                        context.PhysicalMachines.Add(new PhysicalMachine()
                        {
                            CPU = cpuFrequency * cpuCores,
                            Memory = memmory * 1024 * 1024, // kb
                            IOPS = iops * 1024,  // kb/s
                            Network = network * 1024 * 1024 // kbit/s
                        });
                    }
                }
            }
            Logger.EndAction();
        }

        protected override void Seed(SimulationContext context)
        {
            Logger.StartProcess("Mapping Traces Data to DataBase Model");

            CreateTimeEvents(context);
            Logger.LogMessage("TimeEvents created");

            MapFromVMTraces(context);

            context.BulkInsert(removedVMEvents);

            MapPhysicalMachines(context);

            base.Seed(context);

            Logger.EndProccess("Mapping Traces Data to DataBase Model");
        }
    }
}
