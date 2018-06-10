using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace Utilities
{
    public static class GlobalConstants
    {
        /// <summary>
        /// Upper bound for simulation parametrs
        /// Should be always equals or be grater then actual value
        /// </summary>
        public static int TIME_STEPS_CAPACITY = 9000;
        public static int VM_CAPACITY = 3000;
        public static int PM_CAPACITY = 30;

        /// <summary>
        /// Indicate how mach steps will be prognosed
        /// </summary>
        public static int PROGNOSE_DEPTH = 3;

        /// <summary>
        /// simulation step value in ms
        /// </summary>
        public static int TIME_STEP_VALUE = 300;

        #region Evaluation parameters
        /// <summary>
        /// Weight of every resource for comulative value
        /// </summary>
        public static byte CPU_WEIGHT = 1;
        public static byte MEMMORY_WEIGHT = 1;
        public static byte NETWORK_WEIGHT = 1;
        public static byte IOPS_WEIGHT = 1;

        /// <summary>
        /// Used for value normalization
        /// </summary>
        public static float CPU_CAP = 63877f;
        public static float MEMMORY_CAP = 393216f;
        public static float NETWORK_CAP = 879123f;
        public static float IOPS_CAP = 192406f;

        /// <summary>
        /// Percantage of free resource amount.
        /// When free valume is lower then that value
        /// server is considered to be overloaded
        /// </summary>
        public static float CPU_THREADHOLD = 0.1f;
        public static float MEMMORY_THREADHOLD = 0.1f;
        public static float NETWORK_THREADHOLD = 0.1f;
        public static float IOPS_THREADHOLD = 0.1f;

        /// <summary>
        /// Percentage of used resources volume which consider most sutible
        /// </summary>
        public static float CPU_DESIRED_LEVEL = 0.65f;
        public static float MEMMORY_DESIRED_LEVEL = 0.65f;
        public static float NETWORK_DESIRED_LEVEL = 0.65f;
        public static float IOPS_DESIRED_LEVEL = 0.65f;

        /// <summary>
        /// Percentage of used resources volume which consider to be freed for server shut down
        /// </summary>
        public static float CPU_LOW_LEVEL = 0.15f;
        public static float MEMMORY_LOW_LEVEL = 0.15f;
        public static float NETWORK_LOW_LEVEL = 0.15f;
        public static float IOPS_LOW_LEVEL = 0.15f;
        #endregion

        #region Diagnostic parametrs
        /// <summary>
        /// Percantage of free space server should have to became reciever in migration process
        /// </summary>
        public static float CPU_RECIEVER_THREADHOLD = 0.6f;
        public static float MEMMORY_RECIEVER_THREADHOLD = 0.6f;
        public static float NETWORK_RECIEVER_THREADHOLD = 0.6f;
        public static float IOPS_RECIEVER_THREADHOLD = 0.6f;
        #endregion

        #region Migration Parameters
        public static byte MIN_CHILD_NODES_PER_VM = 4;
        public static byte VM_PER_SERVER = 4;
        public static byte BEAM_LENTH = 4;

        public static byte TURN_ON_PENALTY = 20;

        /// <summary>
        /// relative network vaule to use for migration task
        /// </summary>
        public static float NETWORK_ON_MIGRATION = 0.2f;

        /// <summary>
        /// minimal network capacity for migration
        /// </summary>
        public static float MIN_NETWORK_ON_MIGRATION = 100000;

        /// <summary>
        /// minimal network capacity for migration
        /// </summary>
        public static float MAX_NETWORK_ON_MIGRATION = 1000000;  // you should be very carefull. Very big value can broke network loading on migrations

        /// <summary>
        /// absolute CPU resource value to use for migration task
        /// </summary>
        public static float CPU_ON_MIGRATION = 100f;
        #endregion

        #region Prognosing Parameters
        /// <summary>
        /// amount of independent variables in model
        /// </summary>
        public static int INDEPENDENT_VALUES_AMOUNT = 30;

        /// <summary>
        /// max amount of previous values to store
        /// </summary>
        public static int MAX_VALUES_AMOUNT = 120;

        /// <summary>
        /// max difference between prognosed and real val
        /// </summary>
        public static int MAX_DEVIATION = 30000;
        #endregion

        #region Asigning Parameters
        /// <summary>
        /// !!! NOT USED !!!
        /// If vm to asign count is bigger then that param FirstFitDecreasing will be used instead of BestFitDecreasing
        /// </summary>
        public static int BEST_FIT_THREADHOLD = 0;
        #endregion

        public static void LoadFromFile(string filePath)
        {
            Regex regex = new Regex(@"(?<name>\w+) = (?<value>\S+)");
            var type = typeof(GlobalConstants);
            foreach (var line in File.ReadLines(filePath))
            {
                if (regex.IsMatch(line))
                {
                    var match = regex.Match(line);
                    var fieldName = match.Groups["name"].Value;
                    var fieldValue = match.Groups["value"].Value;

                    var fieldType = type.GetField(fieldName).FieldType;

                    type.GetField(fieldName).SetValue(null, ResolveValueType(fieldType, fieldValue));
                }
            }
        }

        private static object ResolveValueType(Type fieldType, string value)
        {
            if (fieldType == typeof(int))
            {
                return int.Parse(value);
            }
            if (fieldType == typeof(float))
            {
                return float.Parse(value, CultureInfo.InvariantCulture);
            }
            if (fieldType == typeof(byte))
            {
                return byte.Parse(value);
            }
            return value;
        }
    }
}
