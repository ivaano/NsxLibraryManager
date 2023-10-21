using System.ComponentModel;
using System.Globalization;
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
                        opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.ReleaseDate, opt => opt.ConvertUsing(new DateTimeConverter()));

        CreateMap<RegionTitle, LibraryTitle>()
                .ForMember(dest => dest.Id,
                        opt => opt.Ignore())
                .ForMember(dest => dest.TitleName,
                        opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Nsuid,
                        opt => opt.MapFrom(src => src.Id));
    }
}

public class DateTimeConverter : IValueConverter<int?, DateTime>
{
    public DateTime Convert(int? sourceMember, ResolutionContext context)
    {
        if (sourceMember == null)
        {
            return new DateTime();
        }

        return DateTime.TryParseExact(sourceMember.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None,  out var parsedDate) ? parsedDate : new DateTime();
    }
}