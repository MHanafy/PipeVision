using System;
using System.Collections.Generic;

namespace PipeVision.Domain
{
    public enum LogStatus
    {
        Unknown,
        Parsed,
        NotTestsFound,
        NotFound,
        NoParserFound
    }

    public interface IPipelineJob : IApplicable<PipelineJob, int>
    {
        DateTime EndDate { get; set; }
        string Name { get; set; }
        Pipeline Pipeline { get; set; }
        int PipelineId { get; set; }
        string Result { get; set; }
        int StageCounter { get; set; }
        string StageName { get; set; }
        DateTime StartDate { get; set; }
        List<Test> Tests { get; set; }
        TestType TestType { get; set; }
        LogStatus LogStatus { get; set; }
    }
}