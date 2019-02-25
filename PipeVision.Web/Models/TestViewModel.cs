using System;
using System.Collections.Generic;

namespace PipeVision.Web.Models
{
    public class TestViewModel
    {
        /// <summary>
        /// Returns the test name only without the name space.
        /// </summary>
        public string ShortName
        {
            get
            {
                var braceIdx = Name.IndexOf('(');
                var dotIdx = braceIdx > -1 ? Name.Substring(0, braceIdx).LastIndexOf('.') : Name.LastIndexOf('.');
                return dotIdx >= 0 ? Name.Substring(dotIdx + 1) : Name;
            }
        }
        public string Name { get; set; }
        public string Error { get; set; }
        public string JobName { get; set; }
        public string StageName { get; set; }
        public string PipelineName { get; set; }
        public DateTime StartTime { get; set; }
    }

    public class ChangeListViewModel
    {
        public int Id { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string UserName { get; set; }
        public string Comment { get; set; }
    }

    public class TestRunViewModel
    {
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string Error { get; set; }
        public string CallStack { get; set; }
        public string Agent { get; set; }
        public int Count { get; set; }
        public List<ChangeListViewModel> ChangeLists { get; set; }
    }

    public class TestDetailedViewModel
    {
        public TestViewModel Test { get; set; }
        public List<TestRunViewModel> TestRuns { get; set; }
    }
}
