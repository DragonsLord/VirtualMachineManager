using System.Collections.Generic;
using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.Services
{
    public interface IReportService
    {
        void Initialize(IEnumerable<int> serverIds);

        void WriteServerStatistics(int step, Server server);

        void DrawCharts();

        void Save();
    }
}
