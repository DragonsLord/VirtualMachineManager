﻿using Simulation.Models;
using Simulation.Models.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Simulation.Modules.Prognosing
{
    public class PrognoseModule
    {
        public void Run(VMCollection vms)
        {
            Logger.StartProcess("Prognosing resources usage");
            foreach (var vm in vms)
            {
                PushResourcesToStatistics(vm);

                Predict(vm);
                //Logger.LogAction($"VM {vm.Id} - done");
            }
            Logger.EndProccess("Prognosing resources usage");
        }

        private void PushResourcesToStatistics(VM vm)
        {
            // TODO: implement (create helper queue class ?)
        }

        private void Predict(VM vm)
        {
            for (int i = 1; i <= GlobalConstants.PROGNOSE_DEPTH; i++)
            {
                vm.UpdatePrognosedRequirments(i, PredictStep(vm, i));
            }
        }

        private Resources PredictStep(VM vm, int step)
        {
            // TODO: implement
            return new Resources();
        }
    }
}
