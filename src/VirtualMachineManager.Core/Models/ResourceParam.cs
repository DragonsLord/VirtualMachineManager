using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualMachineManager.Core.Models
{
    public class ResourceParam<T>
    {
        public T IOPS { get; set; }

        public T Memmory { get; set; }

        public T CPU { get; set; }

        public T Network { get; set; }

        public ResourceParam(T cpu, T memmory, T iops, T network)
        {
            IOPS = iops;
            Memmory = memmory;
            CPU = cpu;
            Network = network;
        }
    }
}
