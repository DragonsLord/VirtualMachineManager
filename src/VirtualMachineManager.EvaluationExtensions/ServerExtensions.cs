using VirtualMachineManager.Core.Models;
using VirtualMachineManager.EvaluationExtensions.Configs;

namespace VirtualMachineManager.EvaluationExtensions
{
    public static class ServerExtensions
    {
        public static ServerEvaluationParams Config { get; } = new ServerEvaluationParams();

        public static bool IsOverloaded(this Server server)
        {
            return server.TurnedOn && IsOverloaded(server, server.UsedResources);
        }
        public static bool IsOverloaded(this Server server, Resources required)
        {
            var aviable = server.ResourcesCapacity - required;
            if (aviable < 0)
                return true;
            return aviable.CPU / server.ResourcesCapacity.CPU < Config.CpuThreahold
                || aviable.Memmory / server.ResourcesCapacity.Memmory < Config.MemoryThreahold
                || aviable.Network / server.ResourcesCapacity.Network < Config.NetworkThreahold
                || aviable.IOPS / server.ResourcesCapacity.IOPS < Config.IopsThreahold;
        }

        public static bool CanRunVM(this Server server, VM vm)
        {
            var required = server.UsedResources + vm.Resources;
            return !IsOverloaded(server, required);
        }
    }
}
