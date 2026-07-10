using Application.Places;
using Application.Visits;
using AutoMapper;
using Domain;
using System.Linq;

namespace Application.Core
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Place, Place>();
            CreateMap<PlaceDto, Place>()
            .ForMember(d => d.CreatedAt, o => o.Ignore());
            CreateMap<Place, PlaceDto>()
            .ForMember(d => d.OwnerUsername, o => o.MapFrom(s => s.Favorites.Where(x => x.IsOwner).Select(x => x.User.UserName).FirstOrDefault()))
            .ForMember(d => d.Image, o => o.MapFrom(s => s.Photos.Select(x => x.Url).FirstOrDefault()));

            CreateMap<FavoritePlace, FavoriteDto>()
            .ForMember(d => d.DisplayName, o => o.MapFrom(s => s.User.DisplayName))
            .ForMember(d => d.Username, o => o.MapFrom(s => s.User.UserName))
            .ForMember(d => d.Bio, o => o.MapFrom(s => s.User.Bio))
            .ForMember(d => d.Image, o => o.MapFrom(s => s.User.Photos.Where(x => x.IsMain).Select(x => x.Url).FirstOrDefault()));

            CreateMap<ApplicationUser, Profiles.Profile>()
                .ForMember(d => d.Image, o => o.MapFrom(s => s.Photos.Where(x => x.IsMain).Select(x => x.Url).FirstOrDefault()));

            CreateMap<Visit, Visit>();
            CreateMap<VisitDto, Visit>()
            .ForMember(d => d.CreatedAt, o => o.Ignore());
            CreateMap<Visit, VisitDto>()
            .ForMember(d => d.DisplayName, o => o.MapFrom(s => s.Author.DisplayName))
            .ForMember(d => d.Username, o => o.MapFrom(s => s.Author.UserName))
            .ForMember(d => d.Image, o => o.MapFrom(s => s.Author.Photos.Where(x => x.IsMain).Select(x => x.Url).FirstOrDefault()));
        }

    }
}
