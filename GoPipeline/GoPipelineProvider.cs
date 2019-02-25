using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GoPipeline.JsonResults;
using Newtonsoft.Json;
using PipeVision.Domain;
using Pipeline = PipeVision.Domain.Pipeline;
using Microsoft.Extensions.Logging;

namespace GoPipeline
{
    public class GoPipelineProvider : IPipelineProvider
    {
        private readonly ILogger<GoPipelineProvider> _logger;
        private readonly HttpClient _client;

        private const string PipelineHistoryUrl = "api/pipelines/{pipelineName}/history/{offset}";
        private const string PipelineInstanceUrl = "api/pipelines/{pipelineName}/instance/{id}";
        private const string PipelineJobLogUrl = "files/{pipelineName}/{pipelineInstance}/{stageName}/{stageId}/{jobName}/cruise-output/console.log";
        private readonly GoJobLogParser _logParser;

        public GoPipelineProvider(string goBaseAddress, string userName, string pass, ILogger<GoPipelineProvider> logger)
        {
            _logger = logger;
            _logParser = new GoJobLogParser(logger);
            _client = new HttpClient {BaseAddress = new Uri(goBaseAddress)};
            var byteArray = Encoding.ASCII.GetBytes($"{userName}:{pass}");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        public async Task<IEnumerable<Pipeline>> GetPipelineRuns(string pipelineName, int startId, int count = 10)
        {
            if (count < 1) throw new ArgumentOutOfRangeException(nameof(count), count, "Must be greater than zero");
            var result = new List<Pipeline>();

            var maxReqId = startId + count - 1;
            var minReqId = startId;

            var offset = 0;
            int currentMinId;
            do
            {
                var url = PipelineHistoryUrl.Replace("{pipelineName}", pipelineName)
                    .Replace("{offset}", offset.ToString());

                //var jsonResult = _client.GetAsync(url).Result;
                var jsonResult = await _client.GetAsync(url);

                var pagedHistory =
                    JsonConvert.DeserializeObject<PipelinePagedHistory>(await jsonResult.Content.ReadAsStringAsync());

                var currentMaxId = pagedHistory.pagination.total - offset;
                currentMinId = currentMaxId - pagedHistory.pagination.page_size + 1;

                var currentTo = Math.Min(currentMaxId, maxReqId);
                var currentFrom = Math.Max(currentMinId, minReqId);
                if(currentTo >= currentFrom) _logger.LogInformation($"Reading from Go Server, Pipeline: {pipelineName} Runs: {currentFrom}-{currentTo}");

                for (var i = currentTo; i >= currentFrom; i--)
                {
                    var index = pagedHistory.pagination.total - pagedHistory.pagination.offset - i;
                    var pipeline = pagedHistory.pipelines[index].ToPipeline();
                    pipeline.PipelineChangeLists = (await GetChangeLists(pagedHistory.pipelines[index].build_cause))
                        .Distinct(new ChangelistIdComparer())
                        .Select(x => new PipelineChangelist {Pipeline = pipeline, ChangeList = x, }).ToList();
                    result.Add(pipeline);
                }

                offset = pagedHistory.pagination.total - Math.Min(currentMinId - 1, maxReqId);

            } while (minReqId < currentMinId);

            return result;
        }

        private async Task<List<ChangeList>> GetChangeLists(BuildCause cause)
        {
            var result = cause.material_revisions
                .Where(x => x.material.type == "Perforce")
                .SelectMany(x => x.GetChangeLists())
                .ToList();

            foreach (var revision in cause.material_revisions.Where(x => x.material.type == "Pipeline"))
            {

                var pipeInfo = Regex.Match(revision.modifications[0].revision, @"^(?'pipeline'.+)\/(?'id'\d+)\/");
                var url = PipelineInstanceUrl.Replace("{pipelineName}", pipeInfo.Groups["pipeline"].Value)
                    .Replace("{id}", pipeInfo.Groups["id"].Value);

                var jsonResult = _client.GetAsync(url).Result;
                var buildCause = JsonConvert.DeserializeObject<JsonResults.Pipeline>(
                    await jsonResult.Content.ReadAsStringAsync()).build_cause;
                result.AddRange(await GetChangeLists(buildCause));
            }

            return result;
        }

        public async Task<string> GetJobLog(string pipelineName, int instanceId, string stageName, int stageId,
            string jobName)
        {
            string url = PipelineJobLogUrl.Replace("{pipelineName}", pipelineName)
                .Replace("{pipelineInstance}", instanceId.ToString())
                .Replace("{stageName}", stageName)
                .Replace("{stageId}", stageId.ToString())
                .Replace("{jobName}", jobName);
            try
            {
                return await _client.GetStringAsync(url);
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("404"))
                    _logger.LogWarning(
                        $"Log file no longer exists, Pipeline: {pipelineName} Stage: {stageName} Job: {jobName}");
                else
                    _logger.LogCritical(ex,
                        $"Unexpected error retrieving log for Pipeline: {pipelineName} Stage: {stageName} Job: {jobName}");
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex,
                    $"Unexpected error retrieving log for Pipeline: {pipelineName} Stage: {stageName} Job: {jobName}");
            }

            return null;
        }

        public async Task<IJobResult> GetJobResult(string pipelineName, int instanceId, string stageName, int stageId,
            string jobName)
        {
            var log = await GetJobLog(pipelineName, instanceId, stageName, stageId, jobName);
            if (log == null) return null;
            return _logParser.Parse(log);
        }
    }
}
