using System.Collections.Generic;

// ReSharper disable InconsistentNaming

namespace PipeVision.GoPipeline.JsonResults
{
    public class Pagination
    {
        public int offset { get; set; }
        public int total { get; set; }
        public int page_size { get; set; }
    }

    public class Stage
    {
        public string result { get; set; }
        public List<Job> jobs { get; set; }
        public string name { get; set; }
        public int? rerun_of_counter { get; set; }
        public string approval_type { get; set; }
        public bool scheduled { get; set; }
        public bool operate_permission { get; set; }
        public string approved_by { get; set; }
        public bool can_run { get; set; }
        public int id { get; set; }
        public int counter { get; set; }
    }

    public enum State
    {
        Completed
    }

    public enum Result
    {
        Passed
    }

    public class Job
    {
        public string state { get; set; }
        public string result { get; set; }
        public string name { get; set; }
        public int id { get; set; }
        public long scheduled_date;
    }

    public class Material
    {
        public string fingerprint { get; set; }
        public string description { get; set; }
        public int id { get; set; }
        public string type { get; set; }
    }

    public class Modification
    {
        public long modified_time { get; set; }
        public string user_name { get; set; }
        public int id { get; set; }
        public string revision { get; set; }
        public string email_address { get; set; }
        public string comment { get; set; }
    }

    public class MaterialRevision
    {
        public Material material { get; set; }
        public List<Modification> modifications { get; set; }
        public bool changed { get; set; }
    }

    public class BuildCause
    {
        public string trigger_message { get; set; }
        public string approver { get; set; }
        public List<MaterialRevision> material_revisions { get; set; }
        public bool trigger_forced { get; set; }
    }

    public class Pipeline
    {
        public string label { get; set; }
        public string name { get; set; }
        public double natural_order { get; set; }
        public bool can_run { get; set; }
        public List<Stage> stages { get; set; }
        public int id { get; set; }
        public BuildCause build_cause { get; set; }
        public bool preparing_to_schedule { get; set; }
        public int counter { get; set; }
        public object comment { get; set; }
    }

    public class PipelinePagedHistory
    {
        public Pagination pagination { get; set; }
        public List<Pipeline> pipelines { get; set; }
    }
}
