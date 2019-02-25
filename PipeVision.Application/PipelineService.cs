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
        /// <returns></returns>
        public async Task<IEnumerable<Test>> GetTestFailures(string testName)
        {
            var timer = new Stopwatch();
            timer.Start();
            var tests = (await _testRepository.GetTestRunsSinceLastSuccess(testName));
            timer.Stop();
            _logger.LogInformation($"Retrieved test failure for '{testName}' : {timer.ElapsedMilliseconds} ms");
            return tests;
        }

        public async Task<List<(Test test, int Count)>> GetUniqueTestFailures(string testName)
        {
            var tests = (await GetTestFailures(testName)).ToList();
            var result= new List<(Test test, int Count)>();
            //below assumes the list is already in a descending order
            (Test test, int Count) current = (null, 0);
            for (var i = tests.Count -1; i >= 0; i--)
            {
                if (tests[i].Error == current.test?.Error)
                {
                    current.Count++;
                    continue;
                }

                if (current.test != null) result.Add(current);
                current = (tests[i], 1);
            }
            if (current.test != null) result.Add(current);

            result.Reverse();
            return result;

        }

        public Task<DateTime?> GetLastSuccessDate(string testName)
        {
            return _testRepository.GetLastSuccessDate(testName);
        }
    }
}
