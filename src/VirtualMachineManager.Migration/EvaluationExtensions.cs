using System;
using System.Collections.Generic;
using System.Text;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.Migration.Model;

namespace VirtualMachineManager.Migration
{
    public static class EvaluationExtensions
    {
        public static Resources GetMigrationResourceRequirments(this Server sender, Server reciever)
        {
            float getServerNetworkReq(Server server)
            {
                return Math.Min(
                    Math.Max(
                        MigrationParams.Current.MinNetworkOnMigration,
                        server.ResourcesCapacity.Network * MigrationParams.Current.NetworkOnMigration
                    ),
                    MigrationParams.Current.MaxNetworkOnMigration
                );
            }
            return new Resources
            {
                CPU = MigrationParams.Current.CpuOnMigration,
                Memmory = 0,
                IOPS = 0,
                Network = Math.Min(getServerNetworkReq(reciever), getServerNetworkReq(sender))
            };
        }
    }
}
