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
            return aviable.CPU / server.ResourcesCapacity.CPU < Config.OverloadThreadhold.CPU
                || aviable.Memmory / server.ResourcesCapacity.Memmory < Config.OverloadThreadhold.Memmory
                || aviable.Network / server.ResourcesCapacity.Network < Config.OverloadThreadhold.Network
                || aviable.IOPS / server.ResourcesCapacity.IOPS < Config.OverloadThreadhold.IOPS;
        }

        public static bool IsLowloaded(this Server server)
        {
            var capacity = server.ResourcesCapacity;
            var load = server.UsedResources;
            return load.CPU <= capacity.CPU * Config.UnderloadThreadhold.CPU &&
            load.Memmory <= capacity.Memmory * Config.UnderloadThreadhold.Memmory &&
            load.Network <= capacity.Network * Config.UnderloadThreadhold.Network &&
            load.IOPS <= capacity.IOPS * Config.UnderloadThreadhold.IOPS;
        }

        public static bool CanRunVM(this Server server, VM vm)
        {
            var required = server.UsedResources + vm.Resources;
            return !IsOverloaded(server, required);
        }
    }
}
