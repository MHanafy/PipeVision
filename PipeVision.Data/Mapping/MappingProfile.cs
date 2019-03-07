using AutoMapper;

namespace PipeVision.Data.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TestRun, Domain.Test>()
                .ForMember(dest => dest.Name, src => src.MapFrom(t => t.Test.Name));

            CreateMap<Domain.Test, TestRun>();
        }
    }
}
