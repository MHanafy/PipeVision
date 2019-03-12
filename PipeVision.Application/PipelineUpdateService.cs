using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Extensions.Logging;
using PipeVision.Domain;

namespace PipeVision.Application
{
    public class PipelineUpdateService : IPipelineUpdateService
    {

        private readonly ITestRepository _testRepository;
        private readonly IPipelineArchiveRepository _pipelineArchiveRepository;
        private readonly IPipelineRepository _pipelineRepository;
        private readonly ILogger<PipelineUpdateService> _logger;
        private readonly IPipelineProvider _pipelineProvider;
        private readonly IList<ITestLogParser> _testLogParsers;

        public PipelineUpdateService(IPipelineRepository pipelineRepository, IPipelineProvider pipelineProvider,
            ILogger<PipelineUpdateService> logger, IList<ITestLogParser> testLogParsers, ITestRepository testRepository, IPipelineArchiveRepository pipelineArchiveRepository)
        {
            _pipelineRepository = pipelineRepository;
            _pipelineProvider = pipelineProvider;
            _logger = logger;
            _testLogParsers = testLogParsers;
            _testRepository = testRepository;
            _pipelineArchiveRepository = pipelineArchiveRepository;
        }

        public async Task UpdatePipelines(string pipelineName, int count = 10)
        {
            try
            {
                _logger.LogInformation($"Starting: Update Pipeline '{pipelineName}' for the last {count} runs");
                var startPipeCounter = (await _pipelineRepository.GetLastUpdatedPipelineCounter(pipelineName) ?? 0) + 1;
                var updatedData = (await _pipelineProvider.GetPipelineRuns(pipelineName, startPipeCounter, count))
                    .ToList();
                _logger.LogInformation($"Found {updatedData.Count} updated pipelines");
                foreach (var pipeline in updatedData)
                {
                    var existingPipeline = await _pipelineRepository.GetPipeline(pipeline.Id);
                    _logger.LogInformation($"Analyzing pipeline {pipeline.Name} instance {pipeline.Counter}");
                    var ignoredJobs = new List<PipelineJob>();
                    foreach (var job in pipeline.PipelineJobs)
                    {
                        var existingJob = existingPipeline?.PipelineJobs?.FirstOrDefault(x => x.Id == job.Id);
                        if (existingJob!=null && existingJob.LogStatus != LogStatus.NotFound && existingJob.LogStatus != LogStatus.Unknown)
                        {
                            _logger.LogInformation($"Skipping stage {job.StageName} - job {job.Name}, Already analyzed.");
                            ignoredJobs.Add(job);
                            continue;
                        }
                        _logger.LogInformation($"Analyzing stage {job.StageName} - job {job.Name}");
                        var result = await _pipelineProvider.GetJobResult(pipelineName, pipeline.Counter, job.StageName,
                            job.StageCounter,
                            job.Name);
                        if (result == null)
                        {
                            job.LogStatus = LogStatus.NotFound;
                            continue;
                        }

                        if (result.StartTime.HasValue) job.StartDate = result.StartTime.Value;
                        if (result.EndTime.HasValue) job.EndDate = result.EndTime.Value;
                        job.Result = result.Status;
                        job.Agent = result.Agent;
                        job.LogStatus = LogStatus.NoParserFound;
                        var testLookup = new Dictionary<string, Test>();
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
                                    job.LogStatus = LogStatus.Parsed;
                                    foreach (var test in tests)
                                    {
                                        //Avoid test result duplication
                                        test.PipelineJobId = job.Id;
                                        if (testLookup.ContainsKey(test.Name))
                                        {
                                            testLookup[test.Name] = test;
                                            _logger.LogWarning(
                                                $"Duplicate test detected in the same run Test: '{test.Name}'");
                                            continue;
                                        }

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
                        if (job.LogStatus == LogStatus.Parsed && job.Tests.Count == 0)
                            job.LogStatus = LogStatus.NotTestsFound;

                    }

                    //Remove all non-updated jobs
                    foreach (var job in ignoredJobs)
                    {
                        pipeline.PipelineJobs.Remove(job);
                    }
                }

                if (updatedData.Count == 0)
                {
                    _logger.LogInformation("Pipeline is up to date!");
                    return;
                }

                _logger.LogInformation(
                    $"Saving pipeline instances {updatedData.Min(x => x.Counter)} to {updatedData.Max(x => x.Counter)}");

                using (var scope = new TransactionScope(TransactionScopeOption.Required,
                    new TransactionOptions {IsolationLevel = IsolationLevel.ReadCommitted}, TransactionScopeAsyncFlowOption.Enabled))
                {
                    
                    await _pipelineRepository.UpdatePipelines(updatedData);
                    await _testRepository.Insert(updatedData.SelectMany(x =>
                        x.PipelineJobs.SelectMany(j => j.Tests??new List<Test>(0))).ToList());
                    scope.Complete();
                }

                _logger.LogInformation("Saving completed");
            }
            catch (Exception e)
            {
                _logger.LogCritical($"Critical error trying to update pipeline {pipelineName}" + e);
            }
        }

        public async Task ArchivePipeLines(int days = 90, int limit = 100)
        {
            try
            {
                var priorToDate = DateTime.Today.AddDays(-1 * days);
                _logger.LogInformation($"Started archiving pipelines prior to {priorToDate}");
                var result = await _pipelineArchiveRepository.Archive(priorToDate, limit);
                _logger.LogInformation($"Finished archiving {result.archiveCount} pipelines, last archived date: {result.lastArchriveDate}");
            }
            catch (Exception e)
            {
                _logger.LogCritical("Critical error trying to archive pipelines: " + e);
            }
        }
    }
}