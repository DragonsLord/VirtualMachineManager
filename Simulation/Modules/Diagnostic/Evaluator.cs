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
                (CPU_WEIGHT * res.CPU + IOPS_WEIGHT * res.IOPS + 
                MEMMORY_WEIGHT * res.Memmory + NETWORK_WEIGHT * res.Network)
                / (CPU_WEIGHT + IOPS_WEIGHT + MEMMORY_WEIGHT + NETWORK_WEIGHT);
        }

        public static bool IsOverloaded(Server server, byte depth)
        {
            return server.TurnedOn && IsOverloaded(server.PrognosedUsedResources[depth], server);
        }

        public static bool IsOverloaded(Resources required, Server server)
        {
            var aviable = server.Resources - required;
            // TODO: add server managing requirments and check prognosed value
            return aviable.CPU / server.Resources.CPU < CPU_THREADHOLD
                || aviable.Memmory / server.Resources.Memmory < MEMMORY_THREADHOLD
                || aviable.Network / server.Resources.Network < NETWORK_THREADHOLD
                || aviable.IOPS / server.Resources.IOPS < IOPS_THREADHOLD;
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
    }
}
