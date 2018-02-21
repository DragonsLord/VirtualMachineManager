using DAL.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simulation.Models.Collections
{
    public class ServerCollection: IEnumerable<Server>
    {
        public int Count => _array.Length;

        private Server[] _array;

        public Server Get(int id) => _array[id - 1];

        public ServerCollection(PhysicalMachineRepository repo)
        {
            var machines = repo.GetAll();
            _array = new Server[machines.Count()];

            foreach (var machine in machines)
            {
                _array[machine.Id - 1] = Server.FromDataBaseModel(machine);
            }
        }

        #region Interface inplementation
        public IEnumerator<Server> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return _array[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        #endregion
    }
}
