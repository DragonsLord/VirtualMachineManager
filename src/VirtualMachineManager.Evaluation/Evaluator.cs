using System;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.Evaluation.Models;

namespace VirtualMachineManager.Evaluation
{
    public class Evaluator : IEvaluator
    {
        private EvaluationParams _params;

        public Evaluator(EvaluationParams settings)
        {
            _params = settings;
        }

        public float Evaluate(Resources res)
        {
            return
                (_params.Weight.CPU * res.CPU / _params.Cap.CPU +
                _params.Weight.IOPS * res.IOPS / _params.Cap.IOPS +
                _params.Weight.Memmory * res.Memmory / _params.Cap.Memmory +
                _params.Weight.Network * res.Network / _params.Cap.Network);
        }

        /*public static bool IsOverloaded(Server server, byte depth)
        {
            return server.TurnedOn && IsOverloaded(server.PrognosedUsedResources[depth], server);
        }*/

        public bool IsServerOverload(Server server, Resources load)
        {
            var aviable = server.ResourcesCapacity - load;
            if (aviable < 0)
                return true;
            return aviable.CPU / server.ResourcesCapacity.CPU < _params.Threadhold.CPU
                || aviable.Memmory / server.ResourcesCapacity.Memmory < _params.Threadhold.Memmory
                || aviable.Network / server.ResourcesCapacity.Network < _params.Threadhold.Network
                || aviable.IOPS / server.ResourcesCapacity.IOPS < _params.Threadhold.IOPS;
        }

        public bool IsServerUnderload(Server server, Resources load)
        {
            var capacity = server.ResourcesCapacity;
            return load.CPU <= capacity.CPU * _params.UnderloadThreadhold.CPU &&
            load.Memmory <= capacity.Memmory * _params.UnderloadThreadhold.Memmory &&
            load.Network <= capacity.Network * _params.UnderloadThreadhold.Network &&
            load.IOPS <= capacity.IOPS * _params.UnderloadThreadhold.IOPS;
        }

        /*public static bool IsNotOverloaded(Server server, byte depth)
        {
            for (int i = 0; i <= depth; i++)
            {
                if (IsOverloaded(server, depth))
                    return false;
            }
            return true;
        }

        public static Resources GetThreadholdDiff(Server server, byte depth)
        {
            var aviable = server.Resources - server.PrognosedUsedResources[depth];
            return new Resources()
            {
                CPU = Math.Abs(server.Resources.CPU * CPU_THREADHOLD - aviable.CPU),
                Memmory = Math.Abs(server.Resources.Memmory * MEMMORY_THREADHOLD - aviable.Memmory),
                Network = Math.Abs(server.Resources.Network * NETWORK_THREADHOLD - aviable.Network),
                IOPS = Math.Abs(server.Resources.IOPS * IOPS_THREADHOLD - aviable.IOPS)
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
            }
            else
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
                return Math.Min(
                    Math.Max(MIN_NETWORK_ON_MIGRATION, server.Resources.Network * NETWORK_ON_MIGRATION),
                    MAX_NETWORK_ON_MIGRATION
                );
            }
            return new Resources
            {
                CPU = CPU_ON_MIGRATION,
                Memmory = 0,
                IOPS = 0,
                Network = Math.Min(getServerNetworkReq(reciever), getServerNetworkReq(sender))
            };
        }*/
    }
}
