using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipeVision.Domain
{
    public interface IPipelineProvider
    {
        /// <summary>
        /// Returns a list of pipeline history greater than or equal startId, limited to count.
        /// </summary>
        /// <param name="pipelineName"></param>
        /// <param name="startId"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        Task<IEnumerable<Pipeline>> GetPipelineRuns(string pipelineName, int startId, int count=10);

        Task<string> GetJobLog(string pipelineName, int instanceId, string stageName, int stageId,
            string jobName);

        Task<IJobResult> GetJobResult(string pipelineName, int instanceId, string stageName, int stageId,
            string jobName);
    }
}
