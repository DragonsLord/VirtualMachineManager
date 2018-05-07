using Simulation.Models;
using System;
using static Utilities.GlobalConstants;

namespace Simulation.Modules.Diagnostic
{
    public static class Evaluator
    {
        public static float Evaluate(Resources res)
        {
            return 
                (CPU_WEIGHT * res.CPU / CPU_CAP + 
                IOPS_WEIGHT * res.IOPS / IOPS_CAP + 
                MEMMORY_WEIGHT * res.Memmory / MEMMORY_CAP + 
                NETWORK_WEIGHT * res.Network / NETWORK_CAP);
        }

        public static bool IsOverloaded(Server server, byte depth)
        {
            return server.TurnedOn && IsOverloaded(server.PrognosedUsedResources[depth], server);
        }

        public static bool IsNotOverloaded(Server server, byte depth)
        {
            for (int i = 0; i <= depth; i++)
            {
                if (IsOverloaded(server, depth))
                    return false;
            }
            return true;
        }

        public static bool IsOverloaded(Resources required, Server server)
        {
            var aviable = server.Resources - required;
            if (aviable < 0)
                return true;
            return aviable.CPU / server.Resources.CPU < CPU_THREADHOLD
                || aviable.Memmory / server.Resources.Memmory < MEMMORY_THREADHOLD
                || aviable.Network / server.Resources.Network < NETWORK_THREADHOLD
                || aviable.IOPS / server.Resources.IOPS < IOPS_THREADHOLD;
        }

        public static Resources GetThreadholdDiff(Server server, byte depth)
        {
            var aviable = server.Resources - server.PrognosedUsedResources[depth];
            return new Resources() {
                CPU = Math.Abs(aviable.CPU - server.Resources.CPU * CPU_THREADHOLD),
                Memmory = Math.Abs(aviable.Memmory - server.Resources.Memmory * MEMMORY_THREADHOLD),
                Network = Math.Abs(aviable.Network - server.Resources.Network * NETWORK_THREADHOLD),
                IOPS = Math.Abs(aviable.IOPS - server.Resources.IOPS * IOPS_THREADHOLD)
            };
        }

        public static float EvaluateForOverloading(Server server, byte depth)
        {
            var usedResources = server.PrognosedUsedResources[depth];
            var toFreeCap = new Resources()
            {
                CPU = server.Resources.CPU * CPU_LOW_LEVEL,
                Memmory = server.Resources.Memmory * MEMMORY_LOW_LEVEL,
                Network = server.Resources.Network * NETWORK_LOW_LEVEL,
                IOPS = server.Resources.IOPS * IOPS_LOW_LEVEL
            };
            if (usedResources < toFreeCap)
            {
                return (toFreeCap - usedResources).EvaluateVolume();
            } else
            {
                var desiredLevel = new Resources()
                {
                    CPU = server.Resources.CPU * CPU_DESIRED_LEVEL,
                    Memmory = server.Resources.Memmory * MEMMORY_DESIRED_LEVEL,
                    Network = server.Resources.Network * NETWORK_DESIRED_LEVEL,
                    IOPS = server.Resources.IOPS * IOPS_DESIRED_LEVEL
                };
                // "-" is used to transform it to maximization task
                return -Math.Abs((desiredLevel - usedResources).EvaluateVolume());
            }
        }

        public static float EvaluateForReleasing(Server server, byte depth)
        {
            var usedResources = server.PrognosedUsedResources[depth];
            var desiredLevel = new Resources()
            {
                CPU = server.Resources.CPU * CPU_DESIRED_LEVEL,
                Memmory = server.Resources.Memmory * MEMMORY_DESIRED_LEVEL,
                Network = server.Resources.Network * NETWORK_DESIRED_LEVEL,
                IOPS = server.Resources.IOPS * IOPS_DESIRED_LEVEL
            };
            // "-" is used to transform it to maximization task
            return -Math.Abs((desiredLevel - usedResources).EvaluateVolume());
        }

        public static bool IsLowLoaded(Server server, byte depth)
        {
            var res = server.PrognosedUsedResources[depth];
            return res.CPU <= server.Resources.CPU * CPU_LOW_LEVEL &&
            res.Memmory <= server.Resources.Memmory * MEMMORY_LOW_LEVEL &&
            res.Network <= server.Resources.Network * NETWORK_LOW_LEVEL &&
            res.IOPS <= server.Resources.IOPS * IOPS_LOW_LEVEL;
        }

        public static Resources GetResourcesUtilization(Server server)
        {
            return new Resources
            {
                CPU = server.UsedResources.CPU / server.Resources.CPU,
                Network = server.UsedResources.Network / server.Resources.Network,
                Memmory = server.UsedResources.Memmory / server.Resources.Memmory,
                IOPS = server.UsedResources.IOPS / server.Resources.IOPS,
            };
        }

        public static Resources GetMigrationResourceRequirments(Server reciever, Server sender)
        {
            float getServerNetworkReq(Server server)
            {
                return Math.Max(MIN_NETWORK_ON_MIGRATION, server.Resources.Network * NETWORK_ON_MIGRATION);
                    //Math.Min(
                    //    Math.Max(MIN_NETWORK_ON_MIGRATION, server.Resources.Network * NETWORK_ON_MIGRATION),
                    //    server.Resources.Network - server.UsedResources.Network // TODO: move this check to diagnostic
                    //);
            }
            return new Resources
            {
                CPU = CPU_ON_MIGRATION,
                Memmory = 0,
                IOPS = 0,
                Network = Math.Min(getServerNetworkReq(reciever), getServerNetworkReq(sender))
            };
        }
    }
}
