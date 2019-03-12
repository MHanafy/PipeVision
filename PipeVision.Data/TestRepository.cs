using Microsoft.EntityFrameworkCore;
using PipeVision.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using AutoMapper;

namespace PipeVision.Data
{
    public class TestRepository : ITestRepository
    {
        private readonly PipelineContext _context;
        private readonly IMapper _mapper;

        public TestRepository(PipelineContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task Insert(IList<Domain.Test> tests, bool ignoreExisting = false)
        {
            using (var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                var testNames = await GetTestIds(tests.Select(x => x.Name).Distinct().ToList(), true);
                foreach (var test in tests)
                {
                     var testId = testNames[test.Name.ToLowerInvariant()];
                    if (ignoreExisting && await _context.TestRuns.FindAsync(testId, test.PipelineJobId) != null) continue;
                    var testRun = _mapper.Map<TestRun>(test);
                    testRun.TestId = testId;
                    await _context.TestRuns.AddAsync(testRun);
                }

                await _context.SaveChangesAsync();
                transaction.Complete();
            }
        }

        private async Task<Dictionary<string, int>> GetTestIds(ICollection<string> names, bool autoCreate)
        {
            var existing = await _context.Tests.Where(x => names.Contains(x.Name)).ToDictionaryAsync(x => x.Name.ToLowerInvariant(), y => y.Id);
            if (!autoCreate || existing.Count == names.Count) return existing;

            var newTests = names.Where(x => !existing.ContainsKey(x.ToLowerInvariant()))
                .Select(name => new Test {Name = name}).ToList();
            await _context.Tests.AddRangeAsync(newTests);
            await _context.SaveChangesAsync();

            foreach (var newTest in newTests)
            {
                existing.Add(newTest.Name.ToLowerInvariant(), newTest.Id);
            }

            return existing;
        }

        private async Task<int> GetTestId(string name, bool checkForExisting)
        {
            if (checkForExisting)
            {
                var exiting = await _context.Tests.Where(x => x.Name == name).FirstOrDefaultAsync();
                if (exiting != null) return exiting.Id;
            }

            var created = await _context.Tests.AddAsync(new Test {Name = name});
            await _context.SaveChangesAsync();
            return created.Entity.Id;
        }

        /// <inheritdoc />
        /// <summary>
        /// Returns a list of currently failing tests, doesn't return a test that was failing but started succeeding
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Domain.Test>> GetFailingTests(DateTime? date = null)
        {
            //Couldn't find an easy way to write below in Linq, hence using a query for now
            var queryString = $@"select t.*
			from {nameof(TestRun)}s t
			join
			(select {nameof(TestRun.TestId)}, max({nameof(TestRun.PipelineJobId)}) {nameof(TestRun.PipelineJobId)}
			from {nameof(TestRun)}s
			group by {nameof(TestRun.TestId)}) keys
			on t.{nameof(TestRun.TestId)} = keys.{nameof(TestRun.TestId)} and t.{nameof(TestRun.PipelineJobId)} = keys.{nameof(TestRun.PipelineJobId)}
			where {nameof(TestRun.Error)} is not null";

            //Get last run for each test
            var query = _context.TestRuns.FromSql(queryString);
            if (date != null) query = query.Where(x => x.PipelineJob.StartDate >= date.Value);

            return await query
                .Include(x => x.Test)
                .Include(x => x.PipelineJob)
                .ThenInclude(x => x.Pipeline)
                .AsNoTracking()
                .Select(x => _mapper.Map<Domain.Test>(x))
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Test>> GetTestRunsSinceLastSuccess(string testName, int maxRecords = 100)
        {
            var lastSuccessJobId = await _context.TestRuns
                .Where(x => x.Test.Name == testName && x.Error == null)
                .Select(x => (int?) x.PipelineJobId)
                .MaxAsync(x => x);

            IQueryable<TestRun> query = lastSuccessJobId.HasValue
                ? _context.TestRuns.Where(x => x.Test.Name == testName && x.PipelineJobId > lastSuccessJobId)
                : _context.TestRuns.Where(x => x.Test.Name == testName);

            return await query
                .OrderByDescending(x => x.PipelineJobId)
                .Take(maxRecords)
                .Include(x => x.Test)
                .Include(x => x.PipelineJob)
                .ThenInclude(x => x.Pipeline)
                .ThenInclude(x => x.PipelineChangeLists)
                .ThenInclude(x => x.ChangeList)
                .AsNoTracking()
                .Select(x => _mapper.Map<Domain.Test>(x))
                .ToListAsync();
        }

        public Task<DateTime?> GetLastSuccessDate(string testName)
        {
            return _context.TestRuns.Where(x => x.Test.Name == testName && x.Error == null)
                .MaxAsync(x => (DateTime?) x.PipelineJob.StartDate);
        }
    }
}
