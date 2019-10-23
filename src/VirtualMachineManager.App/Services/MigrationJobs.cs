using System.Collections.Generic;
using VirtualMachineManager.Migration.Model;

namespace VirtualMachineManager.App.Services
{
    public class MigrationJobs
    {
        private readonly List<MigrationTask> _jobs = new List<MigrationTask>();

        public void Advance()
        {
            for (int i = 0; i < _jobs.Count; i++)
            {
                var job = _jobs[i];
                if (!job.Advance())
                {
                    _jobs.RemoveAt(i);
                    i--;
                }
            }
        }

        public void Add(MigrationTask migrationTask) => _jobs.Add(migrationTask);
    }
}
