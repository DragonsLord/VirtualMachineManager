using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Models;
using Simulation.Models.Collections;
using Simulation.Modules.Diagnostic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Modules.Diagnostic.Tests
{
    [TestClass()]
    public class DiagnosticModuleTests
    {
        [TestMethod()]
        public void DetectOverloadedMachinesTest()
        {
            var collection = new ServerCollection(new List<Server> {
                new Server()
                {
                    TurnedOn = true,
                    InMigration = false,
                    Resources = new Resources
                    {
                        CPU = 1000,
                        Network = 1000,
                        Memmory = 1000,
                        IOPS = 1000
                    },
                    UsedResources = new Resources
                    {
                        CPU = 999,
                        Network = 999,
                        Memmory = 999,
                        IOPS = 999
                    }
                },
                new Server()
                {
                    TurnedOn = true,
                    InMigration = false,
                    Resources = new Resources
                    {
                        CPU = 1000,
                        Network = 1000,
                        Memmory = 1000,
                        IOPS = 1000
                    },
                    UsedResources = new Resources
                    {
                        CPU = 100,
                        Network = 100,
                        Memmory = 100,
                        IOPS = 100
                    }
                },
                new Server()
                {
                    TurnedOn = true,
                    InMigration = false,
                    Resources = new Resources
                    {
                        CPU = 1000,
                        Network = 1000,
                        Memmory = 1000,
                        IOPS = 1000
                    },
                    UsedResources = new Resources
                    {
                        CPU = 800,
                        Network = 800,
                        Memmory = 800,
                        IOPS = 800
                    }
                }
            });

            var result = new DiagnosticModule().DetectOverloadedMachines(collection);

            Assert.AreEqual(1, result.Targets.Count());
            Assert.AreEqual(1, result.Recievers.Count());
        }

        [TestMethod()]
        public void DetectLowloadedMachinesTest()
        {
            var collection = new ServerCollection(new List<Server> {
                new Server()
                {
                    Id = 1,
                    TurnedOn = true,
                    InMigration = false,
                    Resources = new Resources
                    {
                        CPU = 1000,
                        Network = 1000,
                        Memmory = 1000,
                        IOPS = 1000
                    },
                    UsedResources = new Resources
                    {
                        CPU = 999,
                        Network = 999,
                        Memmory = 999,
                        IOPS = 999
                    }
                },
                new Server()
                {
                    Id = 2,
                    TurnedOn = true,
                    InMigration = false,
                    Resources = new Resources
                    {
                        CPU = 1000,
                        Network = 1000,
                        Memmory = 1000,
                        IOPS = 1000
                    },
                    UsedResources = new Resources
                    {
                        CPU = 50,
                        Network = 50,
                        Memmory = 50,
                        IOPS = 50
                    }
                },
                new Server()
                {
                    Id = 3,
                    TurnedOn = true,
                    InMigration = false,
                    Resources = new Resources
                    {
                        CPU = 1000,
                        Network = 1000,
                        Memmory = 1000,
                        IOPS = 1000
                    },
                    UsedResources = new Resources
                    {
                        CPU = 300,
                        Network = 300,
                        Memmory = 300,
                        IOPS = 300
                    }
                }
            });

            var result = new DiagnosticModule().DetectLowloadedMachines(collection);

            Assert.AreEqual(1, result.Targets.Count());
            Assert.AreEqual(1, result.Recievers.Count());
        }
    }
}