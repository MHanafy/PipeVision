using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace PipeVision.Domain
{
    /// <summary>
    /// Represents a pipeline instance (i.e. run)
    /// </summary>
    public class Pipeline : IApplicable<Pipeline, int>
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id{get;set;}
        public int Counter { get; set; }
        public string Name {get;set;}
        public bool InProgress{get;set;} 
        public virtual List<PipelineJob> PipelineJobs{get;set;}   
        public virtual List<PipelineChangelist> PipelineChangeLists { get; set; }

        public void Apply(Pipeline updated)
        {
            InProgress = updated.InProgress;
            PipelineJobs.Apply<PipelineJob, int>(updated.PipelineJobs);
            PipelineChangeLists.Apply<PipelineChangelist, Tuple<int, int>>(updated.PipelineChangeLists);
        }

        public Pipeline Clone()
        {
            var result = (Pipeline) MemberwiseClone();
            result.PipelineJobs = PipelineJobs.Select(x => x.Clone()).ToList();
            return result;
        }
    }

    public enum TestType{
        Unidentified,
        Integration,
        UI
    }
    public class PipelineJob : IPipelineJob
    {
        public void Apply(PipelineJob updated)
        {
            StartDate = updated.StartDate;
            EndDate = updated.EndDate;
            TestType = updated.TestType;
            Result = updated.Result;
            StageName = updated.StageName;
            StageCounter = updated.StageCounter;
            Agent = updated.Agent;
            Tests.Apply<Test, Tuple<int, string>>(updated.Tests);
        }

       [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id{get;set;}
        public string Name{get;set;}
        public DateTime StartDate{get;set;}
        public DateTime EndDate{get;set;}
        public TestType TestType{get;set;}
        public string Result{get;set;}
        public string StageName{get;set;}
        public int StageCounter{get;set;}
        public string Agent { get; set; }
        public int PipelineId{get;set;}
        public virtual Pipeline Pipeline{get;set;}
        public virtual List<Test> Tests{get;set;}

        /// <summary>
        /// Deeply clones all child entities, sets parent entities to null
        /// </summary>
        public PipelineJob Clone()
        {
            var result = (PipelineJob) MemberwiseClone();
            result.Pipeline = null;
            result.Tests = Tests.Select(x => x.Clone()).ToList();
            return result;
        }

    }
    }
