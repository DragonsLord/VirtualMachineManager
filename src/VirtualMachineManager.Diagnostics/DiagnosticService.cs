using System;
using System.Collections.Generic;
using System.Linq;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.Diagnostics.Models;
using VirtualMachineManager.EvaluationExtensions;
using VirtualMachineManager.Services;

namespace VirtualMachineManager.Diagnostics
{
    public class DiagnosticService
    {
        private readonly DiagnosticParams _params;
        //private readonly IServerManager _serverManager;

        public DiagnosticService(DiagnosticParams settings, IServerManager serverManager)
        {
            _params = settings;
            //_serverManager = serverManager;
        }

        public DiagnosticResult DetectOverloadedMachines(/*ServerCollection*/IEnumerable<Server> collection)
        {
            //Logger.StartProcess("Diagnosting for overloaded servers");
            var overloaded = collection
                .Where(p => /*!server.InMigration &&*/ p.IsOverloaded())
                .OrderByDescending(s => GetThreadholdDiff(s, s.UsedResources));
            /* var overloadedPotencially = collection.Except(overloadedCurrently)
                .Where(p => !server.InMigration && _evaluator.IsServerOverload(p, p.PrognosedUsage))
                .OrderByDescending(s => GetThreadholdDiff(s.Server, s.PrognosedUsage));
            var overloaded = overloadedCurrently.Concat(overloadedPotencially); */
            var recipients = collection.Except(overloaded);
            // TODO: change result model to return prognosed values if needed
            var result = new DiagnosticResult(
                overloaded.ToList(),
                collection.Where(s => ValidateThreadhold(s, true)).ToList());
            //Logger.EndProccess("Diagnostic");
            return result;
        }

        public DiagnosticResult DetectLowloadedMachines(/*ServerCollection*/IEnumerable<Server> collection)
        {
            var lowLoaded = collection.Where(s => s.TurnedOn)
                .Where((s) => /*!server.InMigration && */ s.IsLowloaded())
                .OrderByDescending((s) => s.UsedResources.GetValue())
                .ThenBy((s) => s.RunningVMs.Count);

            if (!lowLoaded.Any())
            {
                return null; //DiagnosticResult.Empty;
            }

            var recievers = collection
                .Except(lowLoaded).Where(s => s.TurnedOn) //.Where((s) => s.Server.TurnedOn && !lowLoaded.Any((l) => l.Id == s.Id))
                .Where(s => ValidateThreadhold(s, false));

            if (!recievers.Any())
            {
                return null; //DiagnosticResult.Empty;
            }

            var totalFree = recievers.Select(s => s.ResourcesCapacity - s.UsedResources)
                .Aggregate((r1, r2) => r1 + r2);

            int vmsToMigrateCount = lowLoaded.Select(s => s.RunningVMs.Count).Sum();

            if (vmsToMigrateCount == 0)
            {
                return null; // DiagnosticResult.Empty;
            }

            totalFree -= new Resources()
            {
                CPU = totalFree.CPU * _params.Threadhold.CPU + _params.CpuOnMigration * vmsToMigrateCount,
                Memmory = totalFree.Memmory * _params.Threadhold.Memmory,
                Network = totalFree.Network * _params.Threadhold.Network + _params.MaxNetworkOnMigration * vmsToMigrateCount,
                IOPS = totalFree.IOPS * _params.Threadhold.IOPS
            };

            var exclusions = new List<Server>(lowLoaded.Count());
            foreach (var server in lowLoaded)
            {
                var willLeft = totalFree - server.UsedResources;
                if (willLeft < 0)  // TODO: [Error] no need to substruct if < 0 is true
                {
                    exclusions.Add(server);
                    totalFree += new Resources()
                    {
                        CPU = server.ResourcesCapacity.CPU * (1 - _params.Threadhold.CPU) - server.UsedResources.CPU,
                        Memmory = server.ResourcesCapacity.Memmory * (1 - _params.Threadhold.Memmory) - server.UsedResources.Memmory,
                        Network = server.ResourcesCapacity.Network * (1 - _params.Threadhold.Network) - server.UsedResources.Network,
                        IOPS = server.ResourcesCapacity.IOPS * (1 - _params.Threadhold.IOPS) - server.UsedResources.IOPS
                    };
                }
                else totalFree = willLeft;
            }

            if (
                totalFree.Network - _params.MaxNetworkOnMigration * vmsToMigrateCount <= 0
                || totalFree.CPU - _params.CpuOnMigration * vmsToMigrateCount <= 0
                )
            {
                return null; // DiagnosticResult.Empty;
            }

            return new DiagnosticResult(
                lowLoaded.Except(exclusions),
                recievers.Concat(exclusions));
        }

        private float GetThreadholdDiff(Server server, Resources load)
        {
            var aviable = server.ResourcesCapacity - load;
            return new Resources()
            {
                CPU = Math.Abs(server.ResourcesCapacity.CPU * _params.Threadhold.CPU - aviable.CPU),
                Memmory = Math.Abs(server.ResourcesCapacity.Memmory * _params.Threadhold.Memmory - aviable.Memmory),
                Network = Math.Abs(server.ResourcesCapacity.Network * _params.Threadhold.Network - aviable.Network),
                IOPS = Math.Abs(server.ResourcesCapacity.IOPS * _params.Threadhold.IOPS - aviable.IOPS)
            }.GetValue();
        }

        private bool ValidateThreadhold(Server s, bool includeOffline)
        {
            var worstCaseUsage = new Resources
            {
                CPU = Math.Max(s.UsedResources.CPU, s.UsedResources.CPU),
                Network = Math.Max(s.UsedResources.Network, s.UsedResources.Network),
                Memmory = Math.Max(s.UsedResources.Memmory, s.UsedResources.Memmory),
                IOPS = Math.Max(s.UsedResources.IOPS, s.UsedResources.IOPS)
            };

            var freeRes = s.ResourcesCapacity - worstCaseUsage;
            return /*!s.InMigration &&*/ ((!s.TurnedOn && includeOffline) || (
                freeRes.CPU > _params.RecieverThreadhold.CPU * s.ResourcesCapacity.CPU &&
                freeRes.Memmory > _params.RecieverThreadhold.Memmory * s.ResourcesCapacity.Memmory &&
                freeRes.IOPS > _params.RecieverThreadhold.IOPS * s.ResourcesCapacity.IOPS &&
                freeRes.Network > _params.RecieverThreadhold.Network * s.ResourcesCapacity.Network));
        }
    }
}
