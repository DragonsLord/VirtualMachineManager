using System;
using System.Collections.Generic;
using System.Data.Entity;
using DAL.Entities;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using System.IO;
using System.Globalization;
using EntityFramework.BulkInsert.Extensions;

namespace DAL.Context
{
    public class SimulationDbInitializer: DropCreateDatabaseAlways<SimulationContext>
    {
        private readonly string inputDataFolder = "InputData";
        private readonly string VMTracesFolder = "Traces";
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

            Logger.StartProcessSection("Reading traces");

            foreach(var trace in tracesDir.GetFiles())
            {
                vmId = int.Parse(trace.PureName());

                var vmTrace = ReadTrace(trace, vmId);

                context.BulkInsert(vmTrace);

                Logger.LogAction($"VM {vmId} - done");
            }

            Logger.EndSection("Reading treaces", "done");
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
                        Memmory = memmory,
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
            Logger.StartProcessSection("Reading PM capacities");
            var rnd = new Random(int.MaxValue / 2);
            for (int i = 0; i < GlobalConstants.PM_CAPACITY; i++)
            {
                context.PhysicalMachines.Add(new PhysicalMachine()
                {
                    CPU = 100000 + (float)rnd.NextDouble() * 1000000,
                    Memmory = 2 * 1024 * 1024 + (float)rnd.NextDouble() * 6 * 1024 * 1024,   // 4 Gigs cap
                    IOPS = 10000 + (float)rnd.NextDouble() * 20000,
                    Network = 2000 + (float)rnd.NextDouble() * 10000
                });
                Logger.LogAction($"PM {i + 1} - done");
            }
            Logger.EndSection("Reading PM capacities");
        }

        protected override void Seed(SimulationContext context)
        {
            Logger.StartProcessSection("Mapping Traces Data to DataBase Model");

            CreateTimeEvents(context);
            Logger.LogAction("TimeEvents created");

            MapFromVMTraces(context);

            context.BulkInsert(removedVMEvents);

            MapPhysicalMachines(context);

            base.Seed(context);

            Logger.EndSection("Mapping Traces Data to DataBase Model");
        }
    }
}
