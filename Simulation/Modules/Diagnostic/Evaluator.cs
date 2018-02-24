using Simulation.Models;
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
            return IsOverloaded(server.UsedResources, server, depth);
        }

        public static bool IsOverloaded(Resources required, Server server, byte depth)
        {
            var aviable = server.PrognosedUsedResources[depth];
            // TODO: add server managing requirments and check prognosed value
            return required.CPU > aviable.CPU * CPU_THREADHOLD
                || required.Memmory > aviable.Memmory * MEMMORY_THREADHOLD
                || required.Network > aviable.Network * NETWORK_THREADHOLD
                || required.IOPS > aviable.IOPS * IOPS_THREADHOLD;
        }
    }
}
