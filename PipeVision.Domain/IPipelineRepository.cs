using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipeVision.Domain
{
    public interface IPipelineRepository
    {
        Task<int?> GetLastUpdatedPipelineCounter(string pipelineName);
        Task<Pipeline> GetPipeline(int id);
        Task AddPipeline(Pipeline pipeline);
        Task UpdatePipeline(Pipeline pipeline, bool saveChanges = true);
        Task UpdatePipelines(IEnumerable<Pipeline> pipeLines);
    }
}
