using Simulation.Models;
using Simulation.Models.Collections;
using Simulation.Modules.Prognosing.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;
using Simulation.Modules.Prognosing.Algorythm;

namespace Simulation.Modules.Prognosing
{
    public class PrognoseModule
    {
        private Dictionary<int, ResourceUsageStatistic> _data;
        private RegressionEngine _regressionEngine;

        public PrognoseModule()
        {
            _data = new Dictionary<int, ResourceUsageStatistic>(GlobalConstants.PM_CAPACITY);
            _regressionEngine = new RegressionEngine();
        }

        public void Run(ServerCollection servers)
        {
            Logger.StartProcess("Prognosing resources usage");
            foreach (var server in servers)
            {
                PushResourcesToStatistics(server);

                Predict(server);
            }
            Logger.EndProccess("Prognosing resources usage");
        }

        private void PushResourcesToStatistics(Server server)
        {
            if (!_data.ContainsKey(server.Id))
            {
                _data.Add(server.Id, new ResourceUsageStatistic(server));  
            }
            _data[server.Id].PushResourcesToStatistics();
        }

        private void Predict(Server server)
        {
            var statistics = _data[server.Id];
            if (statistics.Count < 2 * GlobalConstants.INDEPENDENT_VALUES_AMOUNT) // not enough data
            {
                return;
            }

            var res = statistics.GetPredictedResources(_regressionEngine, GlobalConstants.PROGNOSE_DEPTH);

            for (int i = 0; i < GlobalConstants.PROGNOSE_DEPTH; i++)
            {
                server.UpdatePrognosedRequirments(i + 1, res[i]);
            }
        }
    }
}
