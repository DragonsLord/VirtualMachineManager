using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
{
    public static class GlobalConstants
    {
        public const int TIME_STEPS_CAPACITY = 9000;
        public const int VM_CAPACITY = 3000;
        public const int PM_CAPACITY = 30;

        public const int PROGNOSE_DEPTH = 3;

        public const int TIME_STEP_VALUE = 300;

        public const byte CPU_WEIGHT = 1;
        public const byte MEMMORY_WEIGHT = 1;
        public const byte NETWORK_WEIGHT = 1;
        public const byte IOPS_WEIGHT = 1;
    }
}
