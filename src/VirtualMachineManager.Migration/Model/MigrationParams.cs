﻿using System;
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

        public static MigrationParams Current { get; set; }
    }
}
