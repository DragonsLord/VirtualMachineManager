using VirtualMachineManager.Core.Models;

namespace VirtualMachineManager.Services
{
    public interface IReportService
    {
        void Initialize(int serversCount);

        void WriteServerStatistics(int step, Server server);

        void DrawCharts();

        void Save();
    }
}
