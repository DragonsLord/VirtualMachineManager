using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace DAL.Repositories
{
    public class VMEventRepository
    {
        private DbSet<VMEvent> _dbSet;

        public VMEventRepository(DbSet<VMEvent> dbSet)
        {
            _dbSet = dbSet;
        }

        public VMEvent Get(int id)
        {
            return _dbSet.FirstOrDefault(vm => vm.Id == id);
        }
    }
}
