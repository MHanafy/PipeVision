using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using PipeVision.Domain;

namespace PipeVision.Web.Models
{
    public class PipeVisionMappingProfile : Profile
    {
        public PipeVisionMappingProfile()
        {
            CreateMap<Test, TestViewModel>()
                .ForMember(dest => dest.JobName, src => src.MapFrom(j => j.PipelineJob.Name))
                .ForMember(dest => dest.StartTime, src => src.MapFrom(j => j.PipelineJob.StartDate))
                .ForMember(dest => dest.PipelineName, src => src.MapFrom(j => j.PipelineJob.Pipeline.Name))
                .ForMember(dest => dest.StageName, src => src.MapFrom(j => j.PipelineJob.StageName))
                .ForMember(dest => dest.PipelineRun, src => src.MapFrom(j => j.PipelineJob.Pipeline.Counter))
                .ForMember(dest => dest.StageRun, src => src.MapFrom(j => j.PipelineJob.StageCounter))
                .ForMember(dest => dest.StartTime, src => src.MapFrom(j => j.PipelineJob.StartDate));

            CreateMap<ChangeList, ChangeListViewModel>();

            CreateMap<(Test test, int count), TestRunViewModel>()
                .ForMember(dest => dest.Error, src => src.MapFrom(j => j.test.Error))
                .ForMember(dest => dest.CallStack, src => src.MapFrom(j => j.test.CallStack))
                .ForMember(dest => dest.Duration, src => src.MapFrom(j => j.test.Duration))
                .ForMember(dest => dest.Agent, src => src.MapFrom(j => j.test.PipelineJob.Agent))
                .ForMember(dest => dest.StartTime, src => src.MapFrom(j => j.test.PipelineJob.StartDate))
                .ForMember(dest => dest.Count, src => src.MapFrom(c => c.count))
                .ForMember(dest => dest.ChangeLists,
                    src => src.MapFrom(j =>
                        j.test.PipelineJob.Pipeline.PipelineChangeLists.Select(cl => cl.ChangeList)));

            CreateMap<IEnumerable<(Test test, int count)>,TestDetailedViewModel>()
                .ForMember(dest => dest.Test, src => src.MapFrom(t => t.FirstOrDefault().test))
                .ForMember(dest => dest.TestRuns, src => src.MapFrom(t=>t));

        }
    }
}
