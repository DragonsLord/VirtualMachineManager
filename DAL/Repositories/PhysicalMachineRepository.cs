using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DAL.Repositories
{
    public class PhysicalMachineRepository
    {
        private DbSet<PhysicalMachine> _dbSet;

        public PhysicalMachineRepository(DbSet<PhysicalMachine> dbSet)
        {
            _dbSet = dbSet;
        }

        public IEnumerable<PhysicalMachine> GetAll() => _dbSet.AsEnumerable();
    }
}
