using System;
using System.Collections.Generic;
using System.Text;
using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.EvaluationExtensions.Configs
{
    public class ServerEvaluationParams
    {
        public ResourceParam<float> OverloadThreadhold { get; set; }
        public ResourceParam<float> UnderloadThreadhold { get; set; }
    }
}
