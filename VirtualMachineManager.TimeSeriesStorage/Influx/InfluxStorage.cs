using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtualMachineManager.Core.Models;
using VirtualMachineManager.Prognosing.Models;
using VirtualMachineManager.TimeSeriesStorage.Models;

namespace VirtualMachineManager.TimeSeriesStorage.Influx
{
    public class InfluxStorage: ISeriesStorage, IDisposable
    {
        private readonly InfluxHttpClient influxClient;
        private readonly string db;

        public InfluxStorage(InfluxHttpClient influxHttpClient, string dbName)
        {
            influxClient = influxHttpClient;
            db = dbName;

            influxClient.DropDataBase(db).Wait();
            influxClient.CreateDataBase(db).Wait();
        }

        public Task PushNextRecord(IEnumerable<VM> vms, long timestamp)
        {
            var measures = vms.Select(vm => new ResourcesMeasure(GetVmSeriesName(vm.Id), timestamp, vm.Resources));
            return influxClient.WriteMeasures(db, measures);
        }

        public async Task<IEnumerable<Resources>> GetVMTrace(int vmId, long takeFrom)
        {
            var result = await influxClient.GetSeries(db, GetVmSeriesName(vmId), takeFrom);
            return Parse(result).ToArray();
        }

        public void Dispose()
        {
            influxClient.DropDataBase(db);
            influxClient.Dispose();
        }

        private string GetVmSeriesName(int vmId) => $"vm{vmId}";

        private IEnumerable<Resources> Parse(QueryResult queryResult)
            => queryResult.Results[0].Series[0].Values.Select(
                row => new Resources()
                {
                    CPU = Convert.ToSingle(row[1]),
                    IOPS = Convert.ToSingle(row[2]),
                    Memmory = Convert.ToSingle(row[3]),
                    Network = Convert.ToSingle(row[4])
                });
    }
}
