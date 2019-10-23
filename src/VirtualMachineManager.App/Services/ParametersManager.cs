using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using VirtualMachineManager.Asigning.Models;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.Diagnostics.Models;
using VirtualMachineManager.EvaluationExtensions.Configs;
using VirtualMachineManager.Migration.Model;

namespace VirtualMachineManager.App.Services
{
    public class ParametersManager
    {
        class AllParams
        {
            /// <summary>
            /// Upper bound for simulation parametrs
            /// Should be always equals or be grater then actual value
            /// </summary>
            public int TIME_STEPS_CAPACITY = 9000;
            public int VM_CAPACITY = 3000;
            public int PM_CAPACITY = 30;

            /// <summary>
            /// Indicate how mach steps will be prognosed
            /// </summary>
            public int PROGNOSE_DEPTH = 3;

            /// <summary>
            /// simulation step value in ms
            /// </summary>
            public int TIME_STEP_VALUE = 300;

            /// <summary>
            /// amount of simulation steps
            /// 0 - to remove limitation (will simulate all provided steps)
            /// </summary>
            public int STEPS_TO_SIMULATE = 0;

            #region Evaluation parameters
            /// <summary>
            /// Weight of every resource for comulative value
            /// </summary>
            public int CPU_WEIGHT = 1;
            public int MEMMORY_WEIGHT = 1;
            public int NETWORK_WEIGHT = 1;
            public int IOPS_WEIGHT = 1;

            /// <summary>
            /// Used for value normalization
            /// </summary>
            public float CPU_CAP = 63877f;
            public float MEMMORY_CAP = 393216f;
            public float NETWORK_CAP = 879123f;
            public float IOPS_CAP = 192406f;

            /// <summary>
            /// Percantage of free resource amount.
            /// When free valume is lower then that value
            /// server is considered to be overloaded
            /// </summary>
            public float CPU_THREADHOLD = 0.1f;
            public float MEMMORY_THREADHOLD = 0.1f;
            public float NETWORK_THREADHOLD = 0.1f;
            public float IOPS_THREADHOLD = 0.1f;

            /// <summary>
            /// Percentage of used resources volume which consider most sutible
            /// </summary>
            public float CPU_DESIRED_LEVEL = 0.65f;
            public float MEMMORY_DESIRED_LEVEL = 0.65f;
            public float NETWORK_DESIRED_LEVEL = 0.65f;
            public float IOPS_DESIRED_LEVEL = 0.65f;

            /// <summary>
            /// Percentage of used resources volume which consider to be freed for server shut down
            /// </summary>
            public float CPU_LOW_LEVEL = 0.15f;
            public float MEMMORY_LOW_LEVEL = 0.15f;
            public float NETWORK_LOW_LEVEL = 0.15f;
            public float IOPS_LOW_LEVEL = 0.15f;
            #endregion

            #region Diagnostic parametrs
            /// <summary>
            /// Percantage of free space server should have to became reciever in migration process
            /// </summary>
            public float CPU_RECIEVER_THREADHOLD = 0.6f;
            public float MEMMORY_RECIEVER_THREADHOLD = 0.6f;
            public float NETWORK_RECIEVER_THREADHOLD = 0.6f;
            public float IOPS_RECIEVER_THREADHOLD = 0.6f;
            #endregion

            #region Migration Parameters
            public int MIN_CHILD_NODES_PER_VM = 4;
            public int VM_PER_SERVER = 4;
            public int BEAM_LENTH = 4;

            public int TURN_ON_PENALTY = 20;

            /// <summary>
            /// relative network vaule to use for migration task
            /// </summary>
            public float NETWORK_ON_MIGRATION = 0.2f;

            /// <summary>
            /// minimal network capacity for migration
            /// </summary>
            public float MIN_NETWORK_ON_MIGRATION = 100000;

            /// <summary>
            /// minimal network capacity for migration
            /// </summary>
            public float MAX_NETWORK_ON_MIGRATION = 1000000;  // you should be very carefull. Very big value can broke network loading on migrations

            /// <summary>
            /// absolute CPU resource value to use for migration task
            /// </summary>
            public float CPU_ON_MIGRATION = 100f;
            #endregion

            #region Prognosing Parameters
            /// <summary>
            /// amount of independent variables in model
            /// </summary>
            public int INDEPENDENT_VALUES_AMOUNT = 30;

            /// <summary>
            /// max amount of previous values to store
            /// </summary>
            public int MAX_VALUES_AMOUNT = 120;

            /// <summary>
            /// max difference between prognosed and real val
            /// </summary>
            public int MAX_DEVIATION = 30000;
            #endregion
        }

        private AllParams _allParams;

        public int? StepsToSimulate => _allParams.STEPS_TO_SIMULATE == 0 ? (int?)null : _allParams.STEPS_TO_SIMULATE;

        public AsigningParams GetAsigningParams()
        {
            return new AsigningParams()
            {
                DesiredLoadLevel = new ResourceParam<float>(
                    _allParams.CPU_DESIRED_LEVEL,
                    _allParams.MEMMORY_DESIRED_LEVEL,
                    _allParams.IOPS_DESIRED_LEVEL,
                    _allParams.NETWORK_DESIRED_LEVEL)
            };
        }

        public DiagnosticParams GetDiagnosticParams()
        {
            return new DiagnosticParams()
            {
                Threadhold = new ResourceParam<float>(
                    _allParams.CPU_THREADHOLD,
                    _allParams.MEMMORY_THREADHOLD,
                    _allParams.IOPS_THREADHOLD,
                    _allParams.NETWORK_THREADHOLD),
                RecieverThreadhold = new ResourceParam<float>(
                    _allParams.CPU_RECIEVER_THREADHOLD,
                    _allParams.MEMMORY_RECIEVER_THREADHOLD,
                    _allParams.IOPS_RECIEVER_THREADHOLD,
                    _allParams.NETWORK_RECIEVER_THREADHOLD),
                CpuOnMigration = _allParams.CPU_ON_MIGRATION,
                MaxNetworkOnMigration = _allParams.MAX_NETWORK_ON_MIGRATION
            };
        }

        public MigrationParams GetMigrationParams()
        {
            return new MigrationParams()
            {
                BeamLength = _allParams.BEAM_LENTH,
                MinChildNodesPerVM = _allParams.MIN_CHILD_NODES_PER_VM,
                MaxMigrateCandidatesPerStep = _allParams.VM_PER_SERVER, // TODO: rename
                ServerTurnOnPenalty = _allParams.TURN_ON_PENALTY,
                MinNetworkOnMigration = _allParams.MIN_NETWORK_ON_MIGRATION,
                NetworkOnMigration = _allParams.NETWORK_ON_MIGRATION,
                MaxNetworkOnMigration = _allParams.MAX_NETWORK_ON_MIGRATION,
                CpuOnMigration = _allParams.CPU_ON_MIGRATION,
                LowLevel = new ResourceParam<float>(
                    _allParams.CPU_LOW_LEVEL,
                    _allParams.MEMMORY_LOW_LEVEL,
                    _allParams.IOPS_LOW_LEVEL,
                    _allParams.NETWORK_LOW_LEVEL),
                DesiredLevel = new ResourceParam<float>(
                    _allParams.CPU_DESIRED_LEVEL,
                    _allParams.MEMMORY_DESIRED_LEVEL,
                    _allParams.IOPS_DESIRED_LEVEL,
                    _allParams.NETWORK_DESIRED_LEVEL),
            };
        }

        public ResourcesEvaluationParams GetResourcesEvaluationParams()
        {
            return new ResourcesEvaluationParams()
            {
                Cap = new ResourceParam<float>(
                    _allParams.CPU_CAP,
                    _allParams.MEMMORY_CAP,
                    _allParams.IOPS_CAP,
                    _allParams.NETWORK_CAP),
                Weight = new ResourceParam<float>(
                    _allParams.CPU_WEIGHT,
                    _allParams.MEMMORY_WEIGHT,
                    _allParams.IOPS_WEIGHT,
                    _allParams.NETWORK_WEIGHT)
            };
        }

        public ServerEvaluationParams GetServerEvaluationParams()
        {
            return new ServerEvaluationParams()
            {
                OverloadThreadhold = new ResourceParam<float>(
                    _allParams.CPU_THREADHOLD,
                    _allParams.MEMMORY_THREADHOLD,
                    _allParams.IOPS_THREADHOLD,
                    _allParams.NETWORK_THREADHOLD),
                UnderloadThreadhold = new ResourceParam<float>(
                    _allParams.CPU_LOW_LEVEL,
                    _allParams.MEMMORY_LOW_LEVEL,
                    _allParams.IOPS_LOW_LEVEL,
                    _allParams.NETWORK_LOW_LEVEL)
            };
        }

        public ParametersManager(string filePath) => LoadFromFile(filePath);

        private void LoadFromFile(string filePath)
        {
            Regex regex = new Regex(@"(?<name>\w+) = (?<value>\S+)");
            _allParams = new AllParams();
            var type = _allParams.GetType();
            foreach (Match match in regex.Matches(File.ReadAllText(filePath)))
            {
                var fieldName = match.Groups["name"].Value;
                var fieldValue = match.Groups["value"].Value;

                var fieldType = type.GetField(fieldName).FieldType;

                type.GetField(fieldName).SetValue(_allParams, ResolveValueType(fieldType, fieldValue));
            }
        }

        private object ResolveValueType(Type fieldType, string value)
        {
            if (fieldType == typeof(int))
            {
                return int.Parse(value);
            }
            if (fieldType == typeof(float))
            {
                return float.Parse(value, CultureInfo.InvariantCulture);
            }
            return value;
        }
    }
}
