using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities
{
    //TODO: add migration per server constraint ?
    //TODO: add limitation for differance between prognosed and real values
    public static class GlobalConstants
    {
        /// <summary>
        /// Upper bound for simulation parametrs
        /// Should be always equals or be grater then actual value
        /// </summary>
        public const int TIME_STEPS_CAPACITY = 9000;
        public const int VM_CAPACITY = 3000;
        public const int PM_CAPACITY = 30;

        /// <summary>
        /// Indicate how mach steps will be prognosed
        /// </summary>
        public const int PROGNOSE_DEPTH = 3;

        /// <summary>
        /// simulation step value in ms
        /// </summary>
        public const int TIME_STEP_VALUE = 300;

        #region Evaluation parameters
        /// <summary>
        /// Weight of every resource for comulative value
        /// </summary>
        public const byte CPU_WEIGHT = 1;
        public const byte MEMMORY_WEIGHT = 1;
        public const byte NETWORK_WEIGHT = 1;
        public const byte IOPS_WEIGHT = 1;

        /// <summary>
        /// Used for value normalization
        /// </summary>
        public const float CPU_CAP = 63877f;
        public const float MEMMORY_CAP = 393216f;
        public const float NETWORK_CAP = 879123f;
        public const float IOPS_CAP = 192406f;

        /// <summary>
        /// Percantage of free resource amount.
        /// When free valume is lower then that value
        /// server is considered to be overloaded
        /// </summary>
        public const float CPU_THREADHOLD = 0.1f;
        public const float MEMMORY_THREADHOLD = 0.1f;
        public const float NETWORK_THREADHOLD = 0.1f;
        public const float IOPS_THREADHOLD = 0.1f;

        /// <summary>
        /// Percentage of used resources volume which consider most sutible
        /// </summary>
        public const float CPU_DESIRED_LEVEL = 0.65f;
        public const float MEMMORY_DESIRED_LEVEL = 0.65f;
        public const float NETWORK_DESIRED_LEVEL = 0.65f;
        public const float IOPS_DESIRED_LEVEL = 0.65f;

        /// <summary>
        /// Percentage of used resources volume which consider to be freed for server shut down
        /// </summary>
        public const float CPU_LOW_LEVEL = 0.15f;
        public const float MEMMORY_LOW_LEVEL = 0.15f;
        public const float NETWORK_LOW_LEVEL = 0.15f;
        public const float IOPS_LOW_LEVEL = 0.15f;
        #endregion

        #region Diagnostic parametrs
        /// <summary>
        /// Percantage of free space server should have to became reciever in migration process
        /// </summary>
        public const float CPU_RECIEVER_THREADHOLD = 0.6f;
        public const float MEMMORY_RECIEVER_THREADHOLD = 0.6f;
        public const float NETWORK_RECIEVER_THREADHOLD = 0.6f;
        public const float IOPS_RECIEVER_THREADHOLD = 0.6f;
        #endregion

        #region Migration Parameters
        public const byte MIN_CHILD_NODES_PER_VM = 4;
        public const byte VM_PER_SERVER = 4;
        public const byte BEAM_LENTH = 4;

        public const byte TURN_ON_PENALTY = 20;

        /// <summary>
        /// relative network vaule to use for migration task
        /// </summary>
        public const float NETWORK_ON_MIGRATION = 0.2f; // TODO: from total capacity, check diagnostic

        /// <summary>
        /// minimal network amount for migration
        /// </summary>
        public const float MIN_NETWORK_ON_MIGRATION = 5000; // TODO: Add Max Network Cap

        /// <summary>
        /// absolute CPU resource value to use for migration task
        /// </summary>
        public const float CPU_ON_MIGRATION = 100f;
        #endregion

        #region Prognosing Parameters
        public const int INDEPENDENT_VALUES_AMOUNT = 30;

        public const int MAX_VALUES_AMOUNT = 120;

        /// <summary>
        /// max difference between prognosed and real val
        /// </summary>
        public const int MAX_DEVIATION = 30000;
        #endregion

        #region Asigning Parameters
        /// <summary>
        /// !!! NOT USED !!!
        /// If vm to asign count is bigger then that param FirstFitDecreasing will be used instead of BestFitDecreasing
        /// </summary>
        public const int BEST_FIT_THREADHOLD = 0;
        #endregion
    }
}
