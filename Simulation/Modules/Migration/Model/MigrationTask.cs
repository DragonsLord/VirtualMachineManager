﻿using Simulation.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

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
            float getFreeNetwork(Server server){
                return server.Resources.Network - server.UsedResources.Network;
            }
            Resources = new Resources
            {
                CPU = GlobalConstants.CPU_ON_MIGRATION,
                Memmory = 0,
                IOPS = 0,
                Network = Math.Min(getFreeNetwork(Reciever), getFreeNetwork(Sender)) * GlobalConstants.NETWORK_ON_MIGRATION
            };
        }

        private void InitMigrationOnServers()
        {
            TargetVM.IsMigrating = true;
            for (byte depth = 0; depth < GlobalConstants.PROGNOSE_DEPTH; depth++)
            {
                Sender.PrognosedUsedResources[depth] += Resources;
                Reciever.PrognosedUsedResources[depth] += Resources;
            }
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
            for (byte depth = 0; depth < GlobalConstants.PROGNOSE_DEPTH; depth++)
            {
                Sender.PrognosedUsedResources[depth] -= Resources;
                Reciever.PrognosedUsedResources[depth] -= Resources;
            }

            Sender.RemoveVM(TargetVM);
            Reciever.RunVM(TargetVM);
        }
    }
}