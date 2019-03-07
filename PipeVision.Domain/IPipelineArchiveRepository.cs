using System;
using System.Threading.Tasks;

namespace PipeVision.Domain
{
    public interface IPipelineArchiveRepository
    {
        Task<(int archiveCount, DateTime? lastArchriveDate)> Archive(DateTime priorToDate, int limit = 100);
    }
}
