using System;

namespace PipeVision.Domain
{

    public interface ITest
    {
        string Name { get; set; }
        string Error { get; set; }
        string CallStack { get; set; }
        int PipelineJobId { get; set; }
        TimeSpan Duration { get; set; }
        PipelineJob PipelineJob { get; set; }
    }
    
    public class Test : ITest
    {
        public string Name{get;set;}
        public string Error{get;set;}
        public string CallStack{get;set;}
        public int PipelineJobId { get; set; }
        public TimeSpan Duration { get; set; }
        public virtual PipelineJob PipelineJob{get;set;}
    }

    }
