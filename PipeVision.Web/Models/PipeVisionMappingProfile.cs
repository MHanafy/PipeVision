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
                .ForMember(dest => dest.StartTime, src => src.MapFrom(j => j.PipelineJob.StartDate));

            CreateMap<ChangeList, ChangeListViewModel>();

            CreateMap<Test, TestRunViewModel>()
                .ForMember(dest => dest.ChangeLists, src => src.MapFrom(j => j.PipelineJob.Pipeline.PipelineChangeLists.Select(cl=>cl.ChangeList)))
                .ForMember(dest => dest.Agent, src => src.MapFrom(j => j.PipelineJob.Agent))
                .ForMember(dest => dest.StartTime, src => src.MapFrom(j => j.PipelineJob.StartDate));
            ;
            CreateMap<IEnumerable<Test>,TestDetailedViewModel>()
                .ForMember(dest => dest.Test, src => src.MapFrom(t => t.FirstOrDefault()))
                .ForMember(dest => dest.TestRuns, src => src.MapFrom(t=>t));

        }
    }
}
