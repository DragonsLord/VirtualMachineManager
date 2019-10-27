using System.Collections.Generic;
using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.Services
{
    public interface IReportService
    {
        void Initialize(IEnumerable<int> serverIds);

        void WriteServerStatistics(int step, Server server, IEnumerable<Resources> prognosed);

        void DrawCharts();

        void Save();
    }
}
