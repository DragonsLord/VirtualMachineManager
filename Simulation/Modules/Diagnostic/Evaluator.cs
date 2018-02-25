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
    }
}
