using DAL.Entities;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DAL.Repositories
{
    public class TimeEventRepository
    {
        private DbSet<SimualtionTimeEvent> _dbSet;

        public TimeEventRepository(DbSet<SimualtionTimeEvent> dbSet)
        {
            _dbSet = dbSet;
        }

        public SimualtionTimeEvent Get(int id)
        {
            return _dbSet.FirstOrDefault(e => e.Id == id);
        }

        public int Count() => _dbSet.Count();

        /// <summary>
        /// makes query per every entity
        /// </summary>
        /// <returns></returns>
        public IEnumerable<SimualtionTimeEvent> EnumerateAll()
        {
            var count = Count();
            for (int i = 0; i < count; i++)
            {
                yield return Get(i + 1);
            }
        }
    }
}
