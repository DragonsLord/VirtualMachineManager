using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualMachineManager.Core.Models;

namespace Simulation.Modules.Migration.Model
{
    public class MigrationTask
    {
        public Resources @Resources { get; private set; }
        public Server Sender { get; }
        public Server Reciever { get; }
        public VM TargetVM { get; }

        private int steps = 0;

        public MigrationTask(VM targerVM, Server sender, Server reciever)
        {
            Reciever = reciever;
            TargetVM = targerVM;
            Sender = sender;
            CalculateResources();
            steps = Math.Max(1, (int)Math.Ceiling(TargetVM.Resources.Memmory / Resources.Network)); // for 0 memmory tasks
            InitMigrationOnServers();
            System.Diagnostics.Debug.WriteLine($"migration task created: vm{TargetVM.Id} from {Sender.Id} to {Reciever.Id} for {steps}");
        }

        public void CalculateResources()
        {
            Resources = Evaluator.GetMigrationResourceRequirments(Reciever, Sender);
        }

        private void InitMigrationOnServers()
        {
            TargetVM.IsMigrating = true;
            Sender.UsedResources += Resources;
            Reciever.UsedResources += Resources;

            Sender.SendingCount++;
            Reciever.RecievingCount++;
        }

        public void OnNextTimeEvent(Simulation simulation)
        {
            if (--steps == 0)
            {
                EndMigration();
                simulation.OnNextStep -= this.OnNextTimeEvent;
                System.Diagnostics.Debug.WriteLine($"VM{TargetVM.Id} migrated to {Reciever.Id}");
            }
        }

        private void EndMigration()
        {
            TargetVM.IsMigrating = false;

            Sender.UsedResources -= Resources;
            Reciever.UsedResources -= Resources;

            Sender.RemoveVM(TargetVM);
            Reciever.RunVM(TargetVM);
            Sender.SendingCount--;
            Reciever.RecievingCount--;
        }
    }
}
