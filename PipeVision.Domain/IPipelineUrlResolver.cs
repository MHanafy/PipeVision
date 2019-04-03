namespace PipeVision.Domain
{
    public interface IPipelineUrlResolver
    {
        string GetPipelineHistoryUrl(string pipelineName, int offset = 0);
        string GetPipelineRunUrl(string pipelineName, int runId);
        string GetPipelineJobLogUrl(string pipelineName, int pipelineRunId, string stageName, int stageRunId, string jobName);

        string GetPipelineJobUrl(string pipelineName, int pipelineRun, string stageName, int stageRun, string jobName);
    }
}
