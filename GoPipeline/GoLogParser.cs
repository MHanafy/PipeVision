using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using PipeVision.Domain;

// ReSharper disable StringLiteralTypo

namespace PipeVision.GoPipeline
{

    public class JobResult : IJobResult
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; }
        public string Agent { get; set; }
        public List<IJobTaskResult> Tasks { get; set; }
    }

    public class JobTaskResult : IJobTaskResult
    {
        public string Type { get; set; }
        public string Details { get; set; }
        public string Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Log { get; set; }
    }

    public class GoJobLogParser
    {
        private readonly ILogger _logger;

        public GoJobLogParser(ILogger logger)
        {
            _logger = logger;
        }

        #region regex

        private const string LongDateFormat = "yyyy-MM-dd HH:mm:ss";
        private const string ShortDateFormat = "HH:mm:ss";

        private const string RegexLinePrefix = @"(?'time'\d{2}:\d{2}:\d{2})\.\d{3}\s\[go\]\s";

        public const string RegexJobStarted =
            @"(?m)^(.{3})?" + RegexLinePrefix + @"Job Started\:\s(?'date'\d{4}-\d{2}-\d{2} \d{2}\:\d{2}\:\d{2})";

        public const string RegexJobCompleted =
            @"(?m)^(.{3})?" + RegexLinePrefix + @"Job completed\s.*?\son (?'agent'\w*)";

        public const string RegexJobStatus =
            @"(?m)^(.{3})?" + RegexLinePrefix + @"Current\sjob\sstatus\:\s(?'status'.*)";

        //@"(?s)(.{3})?(?'startTime'\d{2}:\d{2}:\d{2})\.\d{3} \[go\] (?>(?>Task (?'taskType'fetch artifact|\w+)(?'task'.*?))|(?>Start to execute task: \<(?'taskType'fetch artifact|\w+)(?'task'.*?)\>))\r?\n";
        //@"(?s)(?<=\n)(?>.{3})?(?'startTime'\d{2}:\d{2}:\d{2})\.\d{3}\s\[go\] (?>(?>Task: (?'taskType'fetch artifact|\w+)(?'task'.*?)\r?\n)|(?>Start to execute task: \<(?'taskType2'fetch artifact|\w+)(?'task2'.*?) ?\/\>))";
        private const string RegexTaskStart =
            @"(?s)(?<=\n)(?>.{3})?(?'startTime'\d{2}:\d{2}:\d{2})\.\d{3}\s\[go\] (?>(?>Task: (?'taskType'fetch artifact|\w+)(?'task'.*?)\r?\n)|(?>Start to execute task: \<(?'taskType'fetch artifact|\w+)(?'task'.*?) ?\/\>))";

        private const string RegexTaskEnd =
            @"^(?>.{3})?(?'endTime'\d{2}:\d{2}:\d{2})\.\d{3}\s\[go\] (?>Task status: |Current job status: )(?'status'\w+)";
        //@"^(?>.{3})?(?'endTime'\d{2}:\d{2}:\d{2})\.\d{3}\s\[go\]\sTask status:\s(?'status'\w*)";

        public const string RegexTask = RegexTaskStart + "(?'log'.+?)" + RegexTaskEnd;

        #endregion

        private readonly Regex _rJobStarted = new Regex(RegexJobStarted, RegexOptions.Compiled);
        private readonly Regex _rJobCompleted = new Regex(RegexJobCompleted, RegexOptions.Compiled);
        private readonly Regex _rJobStatus = new Regex(RegexJobStatus, RegexOptions.Compiled);
        private readonly Regex _rTask = new Regex(RegexTask, RegexOptions.Compiled | RegexOptions.Multiline);

        public IJobResult Parse(string log)
        {
            //Job re-runs are logged in the same log file, for now we'll get the last run only
            log = GetLastJobRunLog(log);

            var job = new JobResult();
            var match = _rJobStarted.Match(log);
            if (match.Success)
            {
                job.StartTime = DateTime.ParseExact(match.Groups["date"].Value, LongDateFormat, null);
            }

            DateTime time;
            match = _rJobCompleted.Match(log);
            if (match.Success)
            {
                if (job.StartTime.HasValue)
                {
                    time = DateTime.ParseExact(match.Groups["time"].Value, ShortDateFormat, null);
                    job.EndTime = job.StartTime.Value.Date + time.TimeOfDay;

                }

                job.Agent = match.Groups["agent"].Value;
            }

            var matches = _rJobStatus.Matches(log);
            //Get the last status in the log file
            if (matches.Count > 0)
            {
                job.Status = matches[matches.Count - 1].Groups["status"].Value;
            }
            else
            {
                job.Status = "Cancelled";
                _logger.LogWarning($"Couldn't read job status, assuming Cancelled.");
            }

            job.Tasks = new List<IJobTaskResult>();

            matches = _rTask.Matches(log);
            foreach (Match tMatch in matches)
            {
                var task = new JobTaskResult
                {
                    Type = tMatch.Groups["taskType"].Value, Details = tMatch.Groups["task"].Value
                };
                time = DateTime.ParseExact(tMatch.Groups["startTime"].Value, ShortDateFormat, null);
                task.StartTime = job.StartTime.HasValue ? job.StartTime.Value.Date + time.TimeOfDay : time;
                time = DateTime.ParseExact(tMatch.Groups["endTime"].Value, ShortDateFormat, null);
                task.EndTime = job.StartTime.HasValue ? job.StartTime.Value.Date + time.TimeOfDay : time;
                task.Status = tMatch.Groups["status"].Value;
                task.Log = tMatch.Groups["log"].Value;
                job.Tasks.Add(task);
            }

            return job;
        }

        private string GetLastJobRunLog(string log)
        {
            var matches = _rJobStarted.Matches(log);
            if (matches.Count <= 1) return log;
            return log.Substring(matches[matches.Count - 1].Index);
        }
    }
}
