using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PipeVision.Domain;

namespace PipeVision.Data
{
    public class PipelineContext : DbContext
    {
        public PipelineContext(DbContextOptions<PipelineContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Test>()
                .HasKey(x => new {x.PipelineJobId, x.Name});

            modelBuilder.Entity<Test>()
                .Property(x => x.Duration)
                .HasConversion(new TimeSpanToTicksConverter());

            modelBuilder.Entity<PipelineChangelist>()
                .HasKey(x => new {x.PipelineId, x.ChangelistId});

            modelBuilder.Entity<PipelineChangelist>()
                .HasOne(x => x.Pipeline)
                .WithMany(x => x.PipelineChangeLists)
                .HasForeignKey(x => x.PipelineId);

            modelBuilder.Entity<PipelineChangelist>()
                .HasOne(x => x.ChangeList)
                .WithMany(x => x.PipelineChangeLists)
                .HasForeignKey(x => x.ChangelistId);

        }

        public DbSet<ChangeList> ChangeLists { get; set; }
        public DbSet<Pipeline> Pipelines { get; set; }
        public DbSet<PipelineJob> PipelineJobs { get; set; }
        public DbSet<Test> Tests { get; set; }
        public DbSet<PipelineChangelist> PipelineChangeLists { get; set; }
    }


}
 