using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using PipeVision.Domain;

namespace PipeVision.Data
{
    public class TestRun
    {
        public int TestId { get; set; }
        public int PipelineJobId { get; set; }
        public string Error { get; set; }
        public string CallStack { get; set; }
        public TimeSpan Duration { get; set; }
        public virtual PipelineJob PipelineJob { get; set; }
        public virtual Test Test { get; set; }
    }

    public class Test
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual List<TestRun> TestRuns { get; set; }
    }
}
