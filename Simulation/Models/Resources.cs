using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Models
{
    public struct Resources
    {
        public float IOPS { get; set; }

        public float Memmory { get; set; }

        public float CPU { get; set; }

        public float Network { get; set; }

        public static Resources operator -(Resources r1, Resources r2)
        {
            return new Resources
            {
                IOPS = r1.IOPS - r2.IOPS,
                Memmory = r1.Memmory - r2.Memmory,
                CPU = r1.CPU - r2.CPU,
                Network = r1.Network - r2.Network
            };
        }

        public static Resources operator +(Resources r1, Resources r2)
        {
            return new Resources
            {
                IOPS = r1.IOPS + r2.IOPS,
                Memmory = r1.Memmory + r2.Memmory,
                CPU = r1.CPU + r2.CPU,
                Network = r1.Network + r2.Network
            };
        }

        public static bool operator <(Resources r1, Resources r2)
        {
            return
                r1.IOPS < r2.IOPS &&
                r1.Memmory < r2.Memmory &&
                r1.CPU < r2.CPU &&
                r1.Network < r2.Network;
        }

        public static bool operator >(Resources r1, Resources r2)
        {
            return
                r1.IOPS > r2.IOPS &&
                r1.Memmory > r2.Memmory &&
                r1.CPU > r2.CPU &&
                r1.Network > r2.Network;
        }
    }
}
