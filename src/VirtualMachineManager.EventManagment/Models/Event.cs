using System;
using System.Collections.Generic;
using System.Text;

namespace VirtualMachineManager.EventManagment.Models
{
    public class Event<T>
    {
        private event Action<T> _event;

        public void Subscribe(Action<T> handler)
        {
            _event += handler;
        }

        public void Raise(T vms)
        {
            _event?.Invoke(vms);
        }

        public void Unsubscribe(Action<T> handler)
        {
            _event -= handler;
        }
    }
}
