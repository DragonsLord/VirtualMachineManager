using Microsoft.VisualStudio.TestTools.UnitTesting;
using Simulation.Models;
using Simulation.Modules.Diagnostic;
using Simulation.Modules.Migration;
using Simulation.Modules.Prognosing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Modules.Prognosing.Tests
{
    [TestClass()]
    public class PrognoseModuleTests
    {
        [TestMethod()]
        public void MigrateFromOverloadedTest()
        {
            var input = new DiagnosticResult(
                new[]
                    {
                        new Server()
                        {
                            TurnedOn = true,
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
                            },
                            RunningVMs = new List<VM>
                            {
                                new VM
                                {
                                    Resources = new Resources
                                    {
                                        CPU = 333,
                                        Network = 333,
                                        Memmory = 333,
                                        IOPS = 333
                                    }
                                },
                                new VM
                                {
                                    Resources = new Resources
                                    {
                                        CPU = 333,
                                        Network = 333,
                                        Memmory = 333,
                                        IOPS = 333
                                    }
                                },
                                new VM
                                {
                                    Resources = new Resources
                                    {
                                        CPU = 333,
                                        Network = 333,
                                        Memmory = 333,
                                        IOPS = 333
                                    }
                                }
                            }
                        }                        
                    },
                new[]
                {
                    new Server()
                        {
                            TurnedOn = true,
                            Resources = new Resources
                            {
                                CPU = 1000,
                                Network = 1000,
                                Memmory = 1000,
                                IOPS = 1000
                            },
                            UsedResources = new Resources
                            {
                                CPU = 0,
                                Network = 0,
                                Memmory = 0,
                                IOPS = 0
                            }
                        }
                },
                0
                );
            var result = new MigrationModule().MigrateFromOverloaded(input);
            Assert.AreEqual(false, result.IsEmpty);
        }

        [TestMethod()]
        public void ReleaseLowloadedMachinesTest()
        {
            var input = new DiagnosticResult(
                new[]
                    {
                        new Server()
                        {
                            TurnedOn = true,
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
                            },
                            RunningVMs = new List<VM>
                            {
                                new VM
                                {
                                    Resources = new Resources
                                    {
                                        CPU = 100,
                                        Network = 100,
                                        Memmory = 100,
                                        IOPS = 100
                                    }
                                }
                            }
                        }
                    },
                new[]
                {
                    new Server()
                        {
                            TurnedOn = true,
                            Resources = new Resources
                            {
                                CPU = 1000,
                                Network = 1000,
                                Memmory = 1000,
                                IOPS = 1000
                            },
                            UsedResources = new Resources
                            {
                                CPU = 0,
                                Network = 0,
                                Memmory = 0,
                                IOPS = 0
                            }
                        }
                },
                0
                );
            var result = new MigrationModule().ReleaseLowloadedMachines(input);
            Assert.AreEqual(1, result.Count);
        }
    }
}