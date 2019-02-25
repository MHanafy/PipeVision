using System;

namespace PipeVision.Domain
{

    public interface ITest : IApplicable<Test, Tuple<int,string>>
    {
        string Name { get; set; }
        string Error { get; set; }
        string CallStack { get; set; }
        int PipelineJobId { get; set; }
        TimeSpan Duration { get; set; }
        PipelineJob PipelineJob { get; set; }
        /// <summary>
        /// Deeply clones all child entities, sets parent entities to null
        /// </summary>
        Test Clone();

    }

    public class Test : ITest
    {
        public void Apply(Test updated)
        {
            Name = updated.Name;
            Error = updated.Error;
            CallStack = updated.CallStack;
            Duration = updated.Duration;
        }

        public string Name{get;set;}
        public string Error{get;set;}
        public string CallStack{get;set;}
        public int PipelineJobId { get; set; }
        public TimeSpan Duration { get; set; }
        public virtual PipelineJob PipelineJob{get;set;}

        /// <summary>
        /// Deeply clones all child entities, sets parent entities to null
        /// </summary>
        public Test Clone()
        {
            var result = (Test) MemberwiseClone();
            result.PipelineJob = null;
            return result;
        }

        public Tuple<int, string> Id => new Tuple<int, string>(PipelineJobId, Name);
    }

    }
