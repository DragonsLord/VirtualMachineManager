using VirtualMachineManager.Prognosing.Models;

namespace VirtualMachineManager.Prognosing
{
    public class PrognosingService
    {
        private readonly ISeriesStorage seriesStorage;

        public PrognosingService(ISeriesStorage seriesStorage)
        {
            this.seriesStorage = seriesStorage;
        }
    }
}
