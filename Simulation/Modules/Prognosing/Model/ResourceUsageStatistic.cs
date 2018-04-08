using Simulation.Models;
using Simulation.Modules.Diagnostic;
using Simulation.Modules.Prognosing.Algorythm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Simulation.Modules.Prognosing.Model
{
    public class ResourceUsageStatistic
    {
        private Server _targetServer;
        private StatisticalDataStream<float> _cpuValuesStream;
        private StatisticalDataStream<float> _memoryValuesStream;
        private StatisticalDataStream<float> _networkValuesStream;
        private StatisticalDataStream<float> _iopsValuesStream;

        public int Count { get; private set; } = 0;

        public ResourceUsageStatistic(Server server)
        {
            _targetServer = server;
            _cpuValuesStream = new StatisticalDataStream<float>(GlobalConstants.MAX_VALUES_AMOUNT);
            _memoryValuesStream = new StatisticalDataStream<float>(GlobalConstants.MAX_VALUES_AMOUNT);
            _networkValuesStream = new StatisticalDataStream<float>(GlobalConstants.MAX_VALUES_AMOUNT);
            _iopsValuesStream = new StatisticalDataStream<float>(GlobalConstants.MAX_VALUES_AMOUNT);
        }

        public void PushResourcesToStatistics()
        {
            var util = Evaluator.GetResourcesUtilization(_targetServer);
            _cpuValuesStream.Push(util.CPU);
            _memoryValuesStream.Push(util.Memmory);
            _networkValuesStream.Push(util.Network);
            _iopsValuesStream.Push(util.IOPS);

            Count++;
        }

        public Resources[] GetPredictedResources(RegressionEngine regression, int steps = GlobalConstants.PROGNOSE_DEPTH)
        {
            var cpu = regression.Run(_cpuValuesStream, steps);
            var memory = regression.Run(_memoryValuesStream, steps);
            var network = regression.Run(_networkValuesStream, steps);
            var iops = regression.Run(_iopsValuesStream, steps);

            var resources = new Resources[steps];

            for (int i = 0; i < steps; i++)
            {
                resources[i] = new Resources
                {
                    CPU = cpu[i] * _targetServer.Resources.CPU,
                    Memmory = memory[i] * _targetServer.Resources.Memmory,
                    Network = network[i] * _targetServer.Resources.Network,
                    IOPS = iops[i] * _targetServer.Resources.IOPS,
                };
            }

            return resources;
        }
    }
}
