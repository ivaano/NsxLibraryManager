using AutoMapper;
using NsxLibraryManager.Models;

namespace NsxLibraryManager.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<TitleDbTitle, RegionTitle>()
            .ForMember(dest => dest.Id,
                opt => opt.MapFrom(src => src.nsuId))
            .ForMember(dest => dest.TitleId,
                opt => opt.MapFrom(src => src.id));
    }
}