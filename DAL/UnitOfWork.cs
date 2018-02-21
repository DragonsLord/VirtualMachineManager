using DAL.Context;
using DAL.Repositories;

namespace DAL
{
    public class DataUnit
    {
        private SimulationContext _simulationContext;

        public PhysicalMachineRepository PhysicalMachineRepository { get; private set; }
        public VMEventRepository VMEventRepository { get; private set; }
        public TimeEventRepository TimeEventRepository { get; private set; }

        public DataUnit()
        {
            _simulationContext = new SimulationContext();

            PhysicalMachineRepository = new PhysicalMachineRepository(_simulationContext.PhysicalMachines);
            VMEventRepository = new VMEventRepository(_simulationContext.VMEvents);
            TimeEventRepository = new TimeEventRepository(_simulationContext.TimeEvents);
        }
    }
}
