using System;
using System.Collections.Generic;
using System.Text;
using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.EvaluationExtensions.Configs
{
    public class ServerEvaluationParams
    {
        public float CpuThreahold { get; set; }
        public float MemoryThreahold { get; set; }
        public float NetworkThreahold { get; set; }
        public float IopsThreahold { get; set; }

        public ResourceParam<float> OverloadThreadhold { get; set; }
        public ResourceParam<float> UnderloadThreadhold { get; set; }
    }
}
