using System.Collections.Generic;
using System.Linq;

namespace VirtualMachineManager.Core.Models
{
    public class Server
    {
        public int Id { get; set; }

        public Resources ResourcesCapacity { get; set; }

        // TODO: [idea] do not store all prognose values and just used the biggest value from prognosing (there should not be any prognose values in this model)
        public Resources UsedResources { get; set; }

        public bool TurnedOn { get; private set; }

        public List<VM> RunningVMs { get; private set; } = new List<VM>();
        
        public void TurnOn()
        {
            TurnedOn = true;
        }

        //TODO: ShoutDown

        public void AsignVM(VM vm)
        {
            RunningVMs.Add(vm);
            UsedResources += vm.Resources;
            vm.AsignToHost(Id, res => UsedResources += res);
        }

        public void RemoveVM(VM vm)
        {
            RunningVMs.Remove(vm);
            UsedResources -= vm.Resources;
            vm.Terminate();
        }

        public Server Copy() =>
            new Server()
            {
                Id = Id,
                ResourcesCapacity = ResourcesCapacity,
                TurnedOn = TurnedOn,
                UsedResources = UsedResources,
                RunningVMs = RunningVMs?.ToList()
            };
    }
}
