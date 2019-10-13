using System;
using System.Collections.Generic;
using System.Text;
using VirtualMachineManager.Migration.Model;

namespace VirtualMachineManager.Migration
{
    public class MigrationManager
    {
        public MigrationManager(MigrationParams config)
        {
            MigrationParams.Current = config;
        }
    }
}
