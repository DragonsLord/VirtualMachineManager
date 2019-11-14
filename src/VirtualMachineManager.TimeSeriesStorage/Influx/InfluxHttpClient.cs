using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VirtualMachineManager.TimeSeriesStorage.Models;

namespace VirtualMachineManager.TimeSeriesStorage.Influx
{
    public class InfluxHttpClient: IDisposable
    {
        private readonly HttpClient http = new HttpClient();
        private readonly string baseUrl;

        public InfluxHttpClient(string baseUrl)
        {
            this.baseUrl = baseUrl;
        }

        public Task CreateDataBase(string dbName) => ExecuteQuery(string.Empty, $"CREATE DATABASE {dbName}");
        public Task DropDataBase(string dbName) => ExecuteQuery(string.Empty, $"DROP DATABASE {dbName}");
        public async Task<QueryResult> GetSeries(string dbName, string measure, long takeFrom = 0) =>
            JsonConvert.DeserializeObject<QueryResult>(await ExecuteQuery(dbName, $"SELECT * FROM {measure} WHERE time > {takeFrom}"));
        public async Task WriteMeasures(string db, IEnumerable<ResourcesMeasure> measures)
        {
            using (var content = new StringContent(GetLineProtocol(measures), Encoding.UTF8))
            {
                using (var response = await http.PostAsync($"{baseUrl}write?db={db}", content))
                {
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        private async Task<string> ExecuteQuery(string db, string query)
        {
            using (var request = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl}query?db={db}&q={query}"))
            {
                using (var res = await http.SendAsync(request))
                {
                    res.EnsureSuccessStatusCode();
                    return await res.Content.ReadAsStringAsync();
                }
            }
        }

        private string GetLineProtocol(IEnumerable<ResourcesMeasure> measures)
        {
            var sb = new StringBuilder();
            foreach(var measure in measures)
            {
                var res = measure.Resources;
                sb.Append($"{measure.Name} cpu={res.CPU},network={res.Network},memory={res.Memmory},iops={res.IOPS} {measure.Timestamp}\n");
            }
            return sb.ToString();
        }

        public void Dispose()
        {
            http.Dispose();
        }
    }
}
