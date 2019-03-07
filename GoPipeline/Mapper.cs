using System;
using System.Collections.Generic;
using System.Linq;
using PipeVision.Domain;
using PipeVision.GoPipeline.JsonResults;
using Pipeline = PipeVision.Domain.Pipeline;

namespace PipeVision.GoPipeline
{
    internal class ChangelistIdComparer : IEqualityComparer<ChangeList>
    {
        public bool Equals(ChangeList x, ChangeList y)
        {
            if (x == null) return y == null;
            return y != null && x.Id.Equals(y.Id);
        }

        public int GetHashCode(ChangeList obj)
        {
            return obj.Id;
        }
    }

    internal static class Mapper
    {
        public static DateTime JavaTimeStampToDateTime(double javaTimeStamp)
        {
            // Java timestamp is milliseconds past epoch
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddMilliseconds(javaTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static IEnumerable<ChangeList> GetChangeLists(this MaterialRevision material)
        {
            return material.modifications.Select(x =>
                new ChangeList
                {
                    Comment = x.comment,
                    Id = int.Parse(x.revision),
                    ModifiedDate = JavaTimeStampToDateTime(x.modified_time),
                    UserName = x.user_name
                });
        }

        public static Pipeline ToPipeline(this global::PipeVision.GoPipeline.JsonResults.Pipeline source, int latestRun)
        {
            return new Pipeline
            {
                Id = source.id,
                Counter = source.counter,
                Name = source.name,
                //Only the latest run is considered in progress
                InProgress = source.counter == latestRun,
                PipelineJobs = source.stages.SelectMany(s => s.jobs.Select(j =>
                    new PipelineJob
                    {
                        Id = j.id,
                        Name = j.name,
                        Result = j.result,
                        PipelineId = source.id,
                        StageName = s.name,
                        StageCounter = s.counter,
                        StartDate = JavaTimeStampToDateTime(j.scheduled_date)
                    }
                )).ToList()
            };

        }

        private static bool IsInProgress(string result)
        {
            return result != "passed" && result != "failed" && result != "cancelled";
        }

        public static List<Pipeline> ToPipelineList(this PipelinePagedHistory pagedHistory)
        {
            return pagedHistory.pipelines.Select(ToPipeline).ToList();
        }
    }
}
