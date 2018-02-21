using DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Context
{
    public class SimulationContext: DbContext
    {
        public DbSet<PhysicalMachine> PhysicalMachines { get; set; }
        public DbSet<VMEvent> VMEvents { get; set; }
        public DbSet<SimualtionTimeEvent> TimeEvents { get; set; }

        static SimulationContext()
        {
            Database.SetInitializer(new SimulationDbInitializer());
        }

        public SimulationContext(): base("SimulationContext") { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new PhysicalMachineConfiguration());
            modelBuilder.Configurations.Add(new VMEventConfiguration());
            modelBuilder.Configurations.Add(new TimeEventConfiguration());
            modelBuilder.Configurations.Add(new RemovedVMEventConfiguration());
        }
    }

    public class PhysicalMachineConfiguration : EntityTypeConfiguration<PhysicalMachine>
    {
        public PhysicalMachineConfiguration() : base()
        {
            HasKey(pm => pm.Id);
            Property(pm => pm.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }

    public class VMEventConfiguration : EntityTypeConfiguration<VMEvent>
    {
        public VMEventConfiguration() : base()
        {
            HasKey(vm => vm.Id);
            Property(vm => vm.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            Property(vm => vm.VMId).IsRequired();

            Property(vm => vm.IsNew).IsRequired();

            HasIndex(vm => vm.TimeEventId);
        }
    }

    public class TimeEventConfiguration : EntityTypeConfiguration<SimualtionTimeEvent>
    {
        public TimeEventConfiguration() : base()
        {
            HasKey(e => e.Id);
            Property(e => e.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            HasMany(e => e.VMEvents)
                .WithRequired()
                .HasForeignKey(vm => vm.TimeEventId);

            HasMany(e => e.RemovedVM)
                .WithRequired()
                .HasForeignKey(re => re.TimeEventId);
        }
    }

    public class RemovedVMEventConfiguration : EntityTypeConfiguration<RemovedVMEvent>
    {
        public RemovedVMEventConfiguration() : base()
        {
            HasKey(re => new { re.TimeEventId, re.VMId });
        }
    }
}
