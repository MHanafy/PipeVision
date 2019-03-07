using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PipeVision.Domain;

namespace PipeVision.Data
{
    public class PipelineArchiveRepository : IPipelineArchiveRepository
    {
        private readonly PipelineContext _context;
        private readonly PipelineArchiveContext _archiveContext;
        private readonly ILogger<PipelineArchiveRepository> _logger;

        public PipelineArchiveRepository(PipelineContext context, PipelineArchiveContext archiveContext, ILogger<PipelineArchiveRepository> logger)
        {
            _context = context;
            _archiveContext = archiveContext;
            _logger = logger;
        }

        public async Task<(int archiveCount, DateTime? lastArchriveDate)> Archive(DateTime priorToDate, int limit = 100)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                var pipeLines = await _context.Pipelines.Where(x => x.PipelineJobs.Max(j => (DateTime?) j.StartDate) < priorToDate)
                    .Include(x => x.PipelineJobs)
                    .Include(x => x.PipelineChangeLists)
                    .ThenInclude(x => x.ChangeList)
                    //This assume change list ids reflect creation date
                    .OrderBy(x=>x.Id)
                    .Take(limit)
                    .ToListAsync();
                _logger.LogInformation($"Retrieved {pipeLines.Count} PipeLines");

                var jobIds = pipeLines.SelectMany(p => p.PipelineJobs.Select(j => j.Id));
                var testRuns = await _context.TestRuns.Where(x => jobIds.Contains(x.PipelineJobId))
                    .Include(x => x.Test)
                    .ToListAsync();
                _logger.LogInformation($"Retrieved {testRuns.Count} TestRuns");

                _archiveContext.Database.UseTransaction(transaction.GetDbTransaction());

                //ignore existing changeLists - It's faster to nullify the property than it's to use DbContext.Entry<>.State
                var changeLists = pipeLines.SelectMany(x => x.PipelineChangeLists).ToList();
                var changeListIds = changeLists.Select(cl => cl.ChangelistId).Distinct().ToList();
                var existingChangelistIds = _archiveContext.ChangeLists.Where(x => changeListIds.Contains(x.Id))
                    .Select(x => x.Id).ToImmutableHashSet();
                foreach (var changeList in changeLists)
                {
                    if (existingChangelistIds.Contains(changeList.ChangelistId)) changeList.ChangeList = null;
                }
                _logger.LogInformation($"Found {changeLists.Count} PipelineChangeLists and {changeListIds.Count} ChangeLists, out of which {existingChangelistIds.Count} already exist" );

                //ignore existing tests
                var allTestIds = testRuns.Select(x => x.TestId).Distinct().ToList();
                var existingTestIds = _archiveContext.Tests.Where(x => allTestIds.Contains(x.Id))
                    .Select(x => x.Id).ToImmutableHashSet();
                foreach (var testRun in testRuns)
                {
                    if (existingTestIds.Contains(testRun.TestId)) testRun.Test = null;
                }
                _logger.LogInformation($"Found {allTestIds.Count} Tests, out of which {existingTestIds.Count} already exist");

                _archiveContext.Pipelines.AddRange(pipeLines);
                _archiveContext.TestRuns.AddRange(testRuns);
                _context.Pipelines.RemoveRange(pipeLines);
                _context.TestRuns.RemoveRange(testRuns);

                await _archiveContext.SaveChangesAsync();
                _logger.LogInformation("Finished saving archived data");
                await _context.SaveChangesAsync();
                _logger.LogInformation("Finished deleting original data");

                transaction.Commit();
                return (pipeLines.Count, pipeLines.Last().PipelineJobs.Max(j=>(DateTime?)j.StartDate));
            }
        }
    }
}
