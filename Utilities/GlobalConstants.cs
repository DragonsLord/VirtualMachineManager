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

        #region Evaluation parameters
        public const byte CPU_WEIGHT = 1;
        public const byte MEMMORY_WEIGHT = 1;
        public const byte NETWORK_WEIGHT = 1;
        public const byte IOPS_WEIGHT = 1;

        // TODO: may be unique for every server
        // percentage of aviable recources
        public const float CPU_THREADHOLD = 0.1f;
        public const float MEMMORY_THREADHOLD = 0.1f;
        public const float NETWORK_THREADHOLD = 0.1f;
        public const float IOPS_THREADHOLD = 0.1f;
        #endregion

        #region Diagnostic parametrs
        public const float CPU_RECIEVER_THREADHOLD = 0.8f;
        public const float MEMMORY_RECIEVER_THREADHOLD = 0.8f;
        public const float NETWORK_RECIEVER_THREADHOLD = 0.8f;
        public const float IOPS_RECIEVER_THREADHOLD = 0.8f;
        #endregion

        #region Migration Parameters
        public const byte MIN_CHILD_NODES_PER_VM = 4;
        public const byte VM_PER_SERVER = 4;
        public const byte BEAM_LENTH = 4;

        public const byte TURN_ON_SHTRAF = 20;
        #endregion
    }
}
