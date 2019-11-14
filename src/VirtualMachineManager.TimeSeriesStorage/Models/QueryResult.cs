using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualMachineManager.TimeSeriesStorage.Models
{
    public class QueryResult
    {
        public class Item
        {
            public Series[] Series { get; set; }
        }
        public class Series
        {
            public string Name { get; set; }
            public string[] Columns { get; set; }
            public object[][] Values { get; set; }
        }
        public Item[] Results { get; set; }
    }
}
