using System;
using System.Collections.Generic;
using System.Text;

namespace PipeVisionConsole.Config
{
    class Group
    {
        public string Name { get; set; }
        public List<string> Pipelines { get; set; }
    }
    class PipelineConfig
    {
        public List<Group> Groups { get; set; }
    }
}
