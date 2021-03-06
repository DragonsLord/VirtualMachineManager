﻿namespace VirtualMachineManager.Core.Models
{
    public struct Resources
    {
        public float IOPS { get; set; }

        public float Memmory { get; set; }

        public float CPU { get; set; }

        public float Network { get; set; }

        public static Resources Empty { get; } = 0;

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

        public static bool operator <(Resources r1, int n) // TODO: check if correct
        {
            return
                r1.IOPS < n ||
                r1.Memmory < n ||
                r1.CPU < n ||
                r1.Network < n;
        }

        public static bool operator >(Resources r1, int n)
        {
            return
                r1.IOPS > n &&
                r1.Memmory > n &&
                r1.CPU > n &&
                r1.Network > n;
        }

        public static implicit operator Resources(int num)
        {
            return new Resources()
            {
                CPU = num,
                Memmory = num,
                Network = num,
                IOPS = num
            };
        }
    }
}
