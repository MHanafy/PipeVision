using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipeVision.Domain
{
    public interface IPipelineUpdateService
    {
        /// <summary>
        /// Analyzes and stores pipeline data, checks last updated pipeline and update incrementally up to count.
        /// </summary>
        /// <param name="pipelineName"></param>
        /// <param name="count"></param>
        Task UpdatePipelines(string pipelineName, int count = 10);

        /// <summary>
        /// Archives pipeline data leaving up provided number of days
        /// </summary>
        /// <param name="days"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task ArchivePipeLines(int days, int limit = 100);
    }

    public interface IPipelineService
    {
        Task<IEnumerable<Test>> GetFailingTests(DateTime? date = null);
        Task<IEnumerable<Test>> GetTestFailures(string testName);
        Task<List<(Test test, int Count)>> GetUniqueTestFailures(string testName);
        Task<DateTime?> GetLastSuccessDate(string testName);
    }
}
