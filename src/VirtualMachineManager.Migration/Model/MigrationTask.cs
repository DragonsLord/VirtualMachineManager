using System;
using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.Migration.Model
{
    public class MigrationTask : IServerJob
    {
        public Resources @Resources { get; private set; }
        public Server Sender { get; }
        public Server Reciever { get; }
        public VM TargetVM { get; }

        private int _steps = 0;

        public MigrationTask(VM targerVM, Server sender, Server reciever)
        {
            Reciever = reciever;
            TargetVM = targerVM;
            Sender = sender;
            Resources = Sender.GetMigrationResourceRequirments(Reciever);
            _steps = Math.Max(1, (int)Math.Ceiling(TargetVM.Resources.Memmory / Resources.Network)); // for 0 memmory tasks
            InitMigrationOnServers();
            // TODO: not debug
            // System.Diagnostics.Debug.WriteLine($"migration task created: vm{TargetVM.Id} from {Sender.Id} to {Reciever.Id} for {_steps}");
        }

        private void InitMigrationOnServers()
        {
            TargetVM.IsMigrating = true;
            Sender.StartJob(this);
            Reciever.StartJob(this);

            Sender.SendingCount++;
            Reciever.RecievingCount++;
        }

        public bool Advance()
        {
            if (--_steps == 0)
            {
                EndMigration();
                // TODO: same here
                // System.Diagnostics.Debug.WriteLine($"VM{TargetVM.Id} migrated to {Reciever.Id}");
                return false;
            }
            return true;
        }

        private void EndMigration()
        {
            TargetVM.IsMigrating = false;

            Sender.FinishJob(this);
            Sender.RemoveVM(TargetVM);
            Sender.SendingCount--;

            Reciever.FinishJob(this);
            Reciever.AsignVM(TargetVM);
            Reciever.RecievingCount--;
        }
    }
}
