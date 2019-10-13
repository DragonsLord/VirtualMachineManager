using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualMachineManager.Migration.Model
{
    public class MigrationParams
    {
        public int MinChildNodesPerVM { get; set; }
        public int MaxMigrateCandidatesPerStep { get; set; }
        public int ServerTurnOnPenalty { get; set; }

        public static MigrationParams Current { get; set; }
    }
}
