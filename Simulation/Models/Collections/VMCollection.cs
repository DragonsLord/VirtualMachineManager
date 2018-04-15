using DAL.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace Simulation.Models.Collections
{
    public class VMCollection : IEnumerable<VM>
    {
        private List<VM> _list = new List<VM>(GlobalConstants.VM_CAPACITY);

        public void Add(VM vm) => _list.Add(vm);

        public void AddRange(IEnumerable<VM> vm) => _list.AddRange(vm);

        public void Remove(VM vm) => _list.Remove(vm);

        public VM Get(int vmId) => _list.FirstOrDefault(vm => vmId == vm.Id);

        public void Update(IEnumerable<VMEvent> vmEvents)
        {
            // TODO: [low] optimize
            foreach (var vme in vmEvents)
            {
                Get(vme.VMId).UpdateRequirments(vme);
            }
        }

        #region Interface implementation
        public IEnumerator<VM> GetEnumerator() => _list.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
        #endregion
    }
}
