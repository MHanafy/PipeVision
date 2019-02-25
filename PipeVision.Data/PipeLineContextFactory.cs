using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PipeVision.Data
{
    public class PipelineContextFactory : IDesignTimeDbContextFactory<PipelineContext>
    {
        const string DesignConnectionString =
            "Server=(localdb)\\mssqllocaldb;Database=PipeVisionTest;Trusted_Connection=True;MultipleActiveResultSets=true";

        public PipelineContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<PipelineContext>();
            optionsBuilder.UseSqlServer(DesignConnectionString);
            return new PipelineContext(optionsBuilder.Options);
        }
    }
}