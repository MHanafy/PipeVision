using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipeVision.Domain;

namespace PipeVision.Application
{
    public class PipelineService : IPipelineService
    {

        private readonly IPipelineRepository _pipelineRepository;
        private readonly ITestRepository _testRepository;
        private readonly ILogger<PipelineService> _logger;

        public PipelineService(IPipelineRepository pipelineRepository, ITestRepository testRepository, ILogger<PipelineService> logger)
        {
            _pipelineRepository = pipelineRepository;
            _testRepository = testRepository;
            _logger = logger;
        }
        
        public async Task<IEnumerable<Test>> GetFailingTests(DateTime? date = null)
        {
            var timer = new Stopwatch();
            timer.Start();
            var result = await _testRepository.GetFailingTests(date);
            timer.Stop();
            _logger.LogInformation($"Retrieved failing tests : {timer.ElapsedMilliseconds} ms");
            return result;
        }

        /// <summary>
        /// Returns a list of all test failures since last known success, with change lists populated
        /// </summary>
        /// <param name="testName"></param>
        /// <param name="keepRedundant">When set to default of false, redundant test runs are omitted, that's runs that fail with the same error consequently</param>
        /// <returns></returns>
        public async Task<IEnumerable<Test>> GetTestFailures(string testName, bool keepRedundant = false)
        {
            var timer = new Stopwatch();
            timer.Start();
            var tests = (await _testRepository.GetTestRunsSinceLastSuccess(testName)).ToList();
            timer.Stop();
            _logger.LogInformation($"Retrieved test failure for '{testName}' : {timer.ElapsedMilliseconds} ms");
            if(keepRedundant) return tests;
            var result= new List<Test>();
            //below assumes the list is already in a descending order
            string lastError = null;
            for (var i = tests.Count -1; i >= 0; i--)
            {
                if (tests[i].Error == lastError) continue;
                result.Add(tests[i]);
                lastError = tests[i].Error;
            }
            return result.OrderByDescending(x=>x.PipelineJob.StartDate);
        }

    }
}
