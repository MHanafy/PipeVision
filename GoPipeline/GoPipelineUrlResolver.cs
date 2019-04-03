using PipeVision.Domain;

namespace PipeVision.GoPipeline
{
    public class GoPipelineUrlResolver : IPipelineUrlResolver
    {
        public string GetPipelineJobUrl(string pipelineName, int pipelineRun, string stageName, int stageRun,
            string jobName) => $"tab/build/detail/{pipelineName}/{pipelineRun}/{stageName}/{stageRun}/{jobName}";
        public string GetPipelineHistoryUrl(string pipelineName, int offset = 0) => $"api/pipelines/{pipelineName}/history/{offset}";

        public string GetPipelineRunUrl(string pipelineName, int runId) => $"api/pipelines/{pipelineName}/instance/{runId}";

        public string GetPipelineJobLogUrl(string pipelineName, int pipelineRunId, string stageName, int stageRunId,
            string jobName) =>
            $"files/{pipelineName}/{pipelineRunId}/{stageName}/{stageRunId}/{jobName}/cruise-output/console.log";
    }
}
