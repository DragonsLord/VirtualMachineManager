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

        public List<IServerJob> Jobs { get; private set; } = new List<IServerJob>();

        public int SendingCount { get; set; }
        public int RecievingCount { get; set; }

        public void TurnOn()
        {
            TurnedOn = true;
        }

        //TODO: ShutDown

        public void StartJob(IServerJob job)
        {
            Jobs.Add(job);
            UsedResources += job.Resources;
        }

        public void FinishJob(IServerJob job)
        {
            Jobs.Remove(job);
        }

        public void AsignVM(VM vm)
        {
            RunningVMs.Add(vm);
            UsedResources += vm.Resources;
            vm.AsignToHost(Id);
        }

        public void RemoveVM(VM vm)
        {
            RunningVMs.Remove(vm);
            vm.Terminate();
        }

        public void UpdateUsedResources()
        {
            UsedResources = RunningVMs.Select(vm => vm.Resources)
                .Concat(Jobs.Select(job => job.Resources))
                .Aggregate(new Resources(), (r, acc) => acc += r);
        }

        public Server Copy() =>
            new Server()
            {
                Id = Id,
                ResourcesCapacity = ResourcesCapacity,
                TurnedOn = TurnedOn,
                UsedResources = UsedResources,
                RunningVMs = RunningVMs.ToList()
            };

        public Server CopyWithoutVms() =>
            new Server()
            {
                Id = Id,
                ResourcesCapacity = ResourcesCapacity,
                Jobs = Jobs.ToList(),
                UsedResources = Jobs.Aggregate(new Resources(), (acc, job) => acc += job.Resources),
                TurnedOn = TurnedOn
            };
    }
}
