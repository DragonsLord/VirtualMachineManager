using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VirtualMachineManager.DataAccess.Traces.Entities;

namespace VirtualMachineManager.DataAccess.Traces
{
    public class TracesDataContext : DbContext
    {
        private string _dbPath;
        public DbSet<PhysicalMachine> PhysicalMachines { get; set; }
        public DbSet<VMEvent> VMEvents { get; set; }
        public DbSet<SimualtionTimeEvent> TimeEvents { get; set; }

        public TracesDataContext(string dbPath)
        {
            _dbPath = dbPath;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Filename={_dbPath}");
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new TimeEventConfiguration());
            modelBuilder.ApplyConfiguration(new PhysicalMachineConfiguration());
            modelBuilder.ApplyConfiguration(new VMEventConfiguration());
            modelBuilder.ApplyConfiguration(new RemovedVMEventConfiguration());
        }
    }

    public class PhysicalMachineConfiguration : IEntityTypeConfiguration<PhysicalMachine>
    {
        public void Configure(EntityTypeBuilder<PhysicalMachine> builder)
        {
            builder.HasKey(pm => pm.Id);
            builder.Property(pm => pm.Id).ValueGeneratedOnAdd();
        }
    }

    public class VMEventConfiguration : IEntityTypeConfiguration<VMEvent>
    {
        public void Configure(EntityTypeBuilder<VMEvent> builder)
        {
            builder.HasKey(re => new { re.TimeEventId, re.VMId });

            builder.Property(vm => vm.VMId).IsRequired();

            builder.Property(vm => vm.IsNew).IsRequired();
        }
    }

    public class TimeEventConfiguration : IEntityTypeConfiguration<SimualtionTimeEvent>
    {
        public void Configure(EntityTypeBuilder<SimualtionTimeEvent> builder)
        {
            builder.HasKey(e => e.Id);

            builder.HasMany(e => e.VMEvents)
                .WithOne()
                .HasForeignKey(vm => vm.TimeEventId);

            builder.HasMany(e => e.RemovedVM)
                .WithOne()
                .HasForeignKey(re => re.TimeEventId);
        }
    }

    public class RemovedVMEventConfiguration : IEntityTypeConfiguration<RemovedVMEvent>
    {
        public void Configure(EntityTypeBuilder<RemovedVMEvent> builder)
        {
            builder.HasKey(re => new { re.TimeEventId, re.VMId });

            builder.Property(re => re.VMId).IsRequired();
        }
    }
}
