using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipeVision.Domain
{
    public interface ITestRepository
    {
        /// <summary>
        /// Returns a list of currently failing tests, doesn't return a test that was failing but started succeeding
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        Task<IEnumerable<Test>> GetFailingTests(DateTime? date = null);
        Task<IEnumerable<Test>> GetTestRunsSinceLastSuccess(string testName, int maxRecords = 100);
        Task<DateTime?> GetLastSuccessDate(string testName);
    }
}