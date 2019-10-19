using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualMachineManager.Migration.Model
{
    public class MigrationParams
    {
        public int BeamLength { get; set; }

        public int MinChildNodesPerVM { get; set; }
        public int MaxMigrateCandidatesPerStep { get; set; }
        public int ServerTurnOnPenalty { get; set; }

        public float MinNetworkOnMigration { get; set; } 
        public float NetworkOnMigration { get; set; } 
        public float MaxNetworkOnMigration { get; set; }
        public float CpuOnMigration { get; set; }


        public float CpuLowLevel { get; set; }
        public float MemoryLowLevel { get; set; }
        public float NetworkLowLevel { get; set; }
        public float IopsLowLevel { get; set; }

        public float CpuDesiredLevel { get; set; }
        public float MemoryDesiredLevel { get; set; }
        public float NetworkDesiredLevel { get; set; }
        public float IopsDesiredLevel { get; set; }

        public static MigrationParams Current { get; set; }
    }
}
