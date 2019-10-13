using System;
using System.Collections.Generic;
using System.Linq;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.Diagnostics.Models;
using VirtualMachineManager.Evaluation;
using VirtualMachineManager.Services;

namespace VirtualMachineManager.Diagnostics
{
    public class DiagnosticService
    {
        private readonly DiagnosticParams _params;
        private readonly IEvaluator _evaluator;
        //private readonly IServerManager _serverManager;

        public DiagnosticService(DiagnosticParams settings, IEvaluator evaluator, IServerManager serverManager)
        {
            _params = settings;
            _evaluator = evaluator;
            //_serverManager = serverManager;
        }

        public DiagnosticResult DetectOverloadedMachines(/*ServerCollection*/IEnumerable<PrognosedServer> collection)
        {
            byte depth = 0;
            //Logger.StartProcess("Diagnosting for overloaded servers");
            var overloadedCurrently = collection
                .Where(p => /*!server.InMigration &&*/ _evaluator.IsServerOverload(p.Server, p.CurrentUsage))
                .OrderByDescending(s => GetThreadholdDiff(s.Server, s.CurrentUsage));
            var overloadedPotencially = collection.Except(overloadedCurrently)
                .Where(p => /*!server.InMigration &&*/ _evaluator.IsServerOverload(p.Server, p.PrognosedUsage))
                .OrderByDescending(s => GetThreadholdDiff(s.Server, s.PrognosedUsage));
            var overloaded = overloadedCurrently.Concat(overloadedPotencially);
            var recipients = collection.Except(overloaded);
            // TODO: change result model to return prognosed values if needed
            var result = new DiagnosticResult(
                overloaded.Select(s => s.Server).ToList(),
                collection.Where(s => ValidateThreadhold(s, true)).Select(s => s.Server).ToList(),
                depth);
            //Logger.EndProccess("Diagnostic");
            return result;
        }

        public DiagnosticResult DetectLowloadedMachines(/*ServerCollection*/IEnumerable<PrognosedServer> collection)
        {
            var lowLoaded = collection.Where(s => s.Server.TurnedOn)
                .Where((s) => /*!server.InMigration && */ _evaluator.IsServerUnderload(s.Server, s.CurrentUsage))
                .OrderByDescending((s) => _evaluator.Evaluate(s.CurrentUsage))
                .ThenBy((s) => s.Server.RunningVMs.Count);

            if (!lowLoaded.Any())
            {
                return null; //DiagnosticResult.Empty;
            }

            var recievers = collection
                .Where((s) => s.Server.TurnedOn && !lowLoaded.Any((l) => l.Id == s.Id))
                .Where(s => ValidateThreadhold(s, false));

            if (!recievers.Any())
            {
                return null; //DiagnosticResult.Empty;
            }

            var totalFree = recievers.Select(s => s.AllResources - s.CurrentUsage)
                .Aggregate((r1, r2) => r1 + r2);

            int vmsToMigrateCount = lowLoaded.Select(s => s.Server.RunningVMs.Count).Sum();

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
                if ((totalFree -= server.PrognosedUsedResources[depth]) < 0)  // TODO: [Error] no need to substruct if < 0 is true
                {
                    exclusions.Add(server);
                    totalFree += new Resources()
                    {
                        CPU = server.Resources.CPU * (1 - _params.Threadhold.CPU) - server.PrognosedUsedResources[depth].CPU,
                        Memmory = server.Resources.Memmory * (1 - _params.Threadhold.Memmory) - server.PrognosedUsedResources[depth].Memmory,
                        Network = server.Resources.Network * (1 - _params.Threadhold.Network) - server.PrognosedUsedResources[depth].Network,
                        IOPS = server.Resources.IOPS * (1 - _params.Threadhold.IOPS) - server.PrognosedUsedResources[depth].IOPS
                    };
                }
            }

            if (
                totalFree.Network - _params.MaxNetworkOnMigration * vmsToMigrateCount <= 0
                || totalFree.CPU - _params.CpuOnMigration * vmsToMigrateCount <= 0
                )
            {
                return null; // DiagnosticResult.Empty;
            }

            return new DiagnosticResult(
                lowLoaded.Where((server) => !exclusions.Any(s => s.Id == server.Id)),
                recievers.Concat(exclusions),
                depth);
        }

        private float GetThreadholdDiff(Server server, Resources load)
        {
            var aviable = server.ResourcesCapacity - load;
            return _evaluator.Evaluate(new Resources()
            {
                CPU = Math.Abs(server.ResourcesCapacity.CPU * _params.Threadhold.CPU - aviable.CPU),
                Memmory = Math.Abs(server.ResourcesCapacity.Memmory * _params.Threadhold.Memmory - aviable.Memmory),
                Network = Math.Abs(server.ResourcesCapacity.Network * _params.Threadhold.Network - aviable.Network),
                IOPS = Math.Abs(server.ResourcesCapacity.IOPS * _params.Threadhold.IOPS - aviable.IOPS)
            });
        }

        private bool ValidateThreadhold(PrognosedServer s, bool includeOffline)
        {
            var worstCaseUsage = new Resources
            {
                CPU = Math.Max(s.CurrentUsage.CPU, s.PrognosedUsage.CPU),
                Network = Math.Max(s.CurrentUsage.Network, s.PrognosedUsage.Network),
                Memmory = Math.Max(s.CurrentUsage.Memmory, s.PrognosedUsage.Memmory),
                IOPS = Math.Max(s.CurrentUsage.IOPS, s.PrognosedUsage.IOPS)
            };

            var freeRes = s.Server.ResourcesCapacity - worstCaseUsage;
            return /*!s.InMigration &&*/ ((!s.Server.TurnedOn && includeOffline) || (
                freeRes.CPU > _params.RecieverThreadhold.CPU * s.AllResources.CPU &&
                freeRes.Memmory > _params.RecieverThreadhold.Memmory * s.AllResources.Memmory &&
                freeRes.IOPS > _params.RecieverThreadhold.IOPS * s.AllResources.IOPS &&
                freeRes.Network > _params.RecieverThreadhold.Network * s.AllResources.Network));
        }
    }
}
