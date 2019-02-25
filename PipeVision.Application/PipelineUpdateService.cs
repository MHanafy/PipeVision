using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipeVision.Domain;

namespace PipeVision.Application
{
    public class PipelineUpdateService : IPipelineUpdateService
    {

        private readonly IPipelineRepository _pipelineRepository;
        private readonly ILogger<PipelineUpdateService> _logger;
        private readonly IPipelineProvider _pipelineProvider;
        private readonly IList<ITestLogParser> _testLogParsers;

        public PipelineUpdateService(IPipelineRepository pipelineRepository, IPipelineProvider pipelineProvider,  ILogger<PipelineUpdateService> logger, IList<ITestLogParser> testLogParsers)
        {
            _pipelineRepository = pipelineRepository;
            _pipelineProvider = pipelineProvider;
            _logger = logger;
            _testLogParsers = testLogParsers;
        }

        public async Task UpdatePipelines(string pipelineName, int count = 10)
        {
            try
            {
                _logger.LogInformation($"Starting: Update Pipeline '{pipelineName}' for the last {count}");
                var startPipeCounter = (await _pipelineRepository.GetLastUpdatedPipelineCounter(pipelineName) ?? 0) + 1;
                var updatedData = (await _pipelineProvider.GetPipelineRuns(pipelineName, startPipeCounter, count)).ToList();
                _logger.LogInformation($"Found {updatedData.Count} updated pipelines");
                foreach (var pipeline in updatedData)
                {
                    _logger.LogInformation($"Analyzing pipeline {pipeline.Name} instance {pipeline.Counter}");
                    foreach (var job in pipeline.PipelineJobs)
                    {
                        _logger.LogInformation($"Analyzing stage {job.StageName} - job {job.Name}");
                        var testLookup = new Dictionary<string, Test>();
                        var result = await _pipelineProvider.GetJobResult(pipelineName, pipeline.Counter, job.StageName,
                            job.StageCounter,
                            job.Name);
                        if (result == null) continue;
                        if (result.StartTime.HasValue) job.StartDate = result.StartTime.Value;
                        if (result.EndTime.HasValue) job.EndDate = result.EndTime.Value;
                        job.Result = result.Status;
                        job.Agent = result.Agent;
                        foreach (var task in result.Tasks)
                        {
                            if (task.Log == null) continue;
                            foreach (var parser in _testLogParsers)
                            {
                                try
                                {
                                    var tests = parser.Parse<Test>(task.Log);
                                    if (tests == null) continue; //Log isn't recognized by the parser
                                    if (!tests.Any())
                                        _logger.LogWarning(
                                            $"Job log doesn't have any test results! Job: {job.Name}");
                                    foreach (var test in tests)
                                    {
                                        //Avoid test result duplication
                                        if (testLookup.ContainsKey(test.Name))
                                        {
                                            testLookup[test.Name] = test;
                                            _logger.LogWarning(
                                                $"Duplicate test detected in the same run Test: '{test.Name}'");
                                            continue;
                                        }
                                        test.PipelineJobId = job.Id;
                                        testLookup.Add(test.Name, test);
                                    }

                                    job.TestType = parser.TestType;
                                    break;
                                }
                                catch (Exception e)
                                {
                                    _logger.LogCritical(e, parser.GetType().Name + " Parsing error");
                                }
                            }
                        }

                        job.Tests = testLookup.Values.ToList();

                    }
                }

                if (updatedData.Count == 0)
                {
                    _logger.LogInformation("Pipeline is up to date!");
                    return;
                }
                _logger.LogInformation($"Saving pipeline instances {updatedData.Min(x => x.Counter)} to {updatedData.Max(x => x.Counter)}");
                await _pipelineRepository.UpdatePipelines(updatedData);
                _logger.LogInformation($"Saving completed");
            }
            catch (Exception e)
            {
                _logger.LogCritical($"Critical error trying to update pipeline {pipelineName}" + e);
            }
        }
    }
}