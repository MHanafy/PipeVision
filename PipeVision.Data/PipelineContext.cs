using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PipeVision.Domain;

namespace PipeVision.Data
{
    public class PipelineContext : DbContext
    {
        public PipelineContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Test>()
                .HasIndex(x => x.Name).IsUnique();

            modelBuilder.Entity<TestRun>()
                .HasKey(x => new {x.TestId, x.PipelineJobId});

            modelBuilder.Entity<TestRun>()
                .Property(x => x.Duration)
                .HasConversion(new TimeSpanToTicksConverter());

            modelBuilder.Entity<PipelineJob>()
                .Ignore(x => x.Tests);

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
        public DbSet<TestRun> TestRuns { get; set; }
        public DbSet<PipelineChangelist> PipelineChangeLists { get; set; }
    }


}
 