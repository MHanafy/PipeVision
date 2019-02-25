using Microsoft.EntityFrameworkCore;
using PipeVision.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PipeVision.Data
{
    public class TestRepository : ITestRepository
    {
        private readonly PipelineContext _context;

        public TestRepository(PipelineContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a list of currently failing tests, doesn't return a test that was failing but started succeeding
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Test>> GetFailingTests(DateTime? date = null)
        {
            //Couldn't find an easy way to write below in Linq, hence using a query for now
            var queryString = $@"select t.*
			from tests t
			join
			(select {nameof(Test.Name)}, max({nameof(Test.PipelineJobId)}) {nameof(Test.PipelineJobId)}
			from tests
			group by {nameof(Test.Name)}) keys
			on t.{nameof(Test.Name)} = keys.{nameof(Test.Name)} and t.{nameof(Test.PipelineJobId)} = keys.{nameof(Test.PipelineJobId)}
			where {nameof(Test.Error)} is not null";

            //Get last run for each test
            var query = _context.Tests.FromSql(queryString);
            if (date != null) query = query.Where(x => x.PipelineJob.StartDate >= date.Value);

            return await query
                .Include(x => x.PipelineJob)
                .ThenInclude(x => x.Pipeline)
                .AsNoTracking()
                .ToListAsync();
        }
        public async Task<IEnumerable<Test>> GetTestRunsSinceLastSuccess(string testName, int maxRecords = 100)
        {
            var lastSuccessJobId = await _context.Tests
                .Where(x => x.Name == testName && x.Error == null)
                .Select(x=> (int?) x.PipelineJobId)
                .MaxAsync(x=>x);

            IQueryable<Test> query = lastSuccessJobId.HasValue
                ? _context.Tests.Where(x => x.Name == testName && x.PipelineJobId > lastSuccessJobId)
                : _context.Tests.Where(x => x.Name == testName);

            return await query
                .OrderByDescending(x => x.PipelineJobId)
                .Take(maxRecords)
                .Include(x => x.PipelineJob)
                .ThenInclude(x => x.Pipeline)
                .ThenInclude(x => x.PipelineChangeLists)
                .ThenInclude(x => x.ChangeList)
                .ToListAsync();
        }

        public Task<DateTime?> GetLastSuccessDate(string testName)
        {
            return _context.Tests.Where(x => x.Name == testName && x.Error == null)
                .MaxAsync(x => (DateTime?) x.PipelineJob.StartDate);
        }
    }
}
