using System;
using System.Collections.Generic;

namespace PipeVision.Domain
{
    public interface IJobResult
    {
        DateTime? StartTime { get; set; }
        DateTime? EndTime { get; set; }
        string Status { get; set; }
        string Agent { get; set; }
        List<IJobTaskResult> Tasks { get; set; }
    }

    public interface IJobTaskResult
    {
        string Type { get; set; }
        string Details { get; set; }
        string Status { get; set; }
        DateTime StartTime { get; set; }
        DateTime EndTime { get; set; }
        string Log { get; set; }
    }
}
