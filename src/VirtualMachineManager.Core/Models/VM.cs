using System;

namespace VirtualMachineManager.Core.Models
{
    public class VM
    {
        private Resources _resources;
        private Action<Resources> _updateHost;
        public int Id { get; set; }

        public bool IsMigrating { get; set; } // Remove ?

        public Resources Resources {
            get => _resources;
            set {
                _updateHost?.Invoke(value - _resources);
                _resources = value;
            }
        }
        
        public int HostId { get; private set; } = 0;

        public void AsignToHost(int hostId, Action<Resources> updateHost)
        {
            HostId = hostId;
            _updateHost = updateHost;
        }

        public void Terminate()
        {
            HostId = 0;
            _updateHost = null;
        }
    }
}
