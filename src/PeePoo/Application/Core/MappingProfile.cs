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
            CreateMap<PlaceDto, Place>();
            CreateMap<Place, PlaceDto>()
            .ForMember(d => d.OwnerUsername, o => o.MapFrom(s => s.Favorites.FirstOrDefault(x => x.IsOwner).User.UserName));
            CreateMap<FavoritePlace, FavoriteDto>()
            .ForMember(d => d.DisplayName, o => o.MapFrom(s => s.User.DisplayName))
            .ForMember(d => d.Username, o => o.MapFrom(s => s.User.UserName))
            .ForMember(d => d.Bio, o => o.MapFrom(s => s.User.Bio))
            .ForMember(d => d.Image, o => o.MapFrom(s => s.User.Photos.FirstOrDefault(x => x.IsMain).Url));

            CreateMap<ApplicationUser, Profiles.Profile>()
                .ForMember(d => d.Image, o => o.MapFrom(s => s.Photos.FirstOrDefault(x => x.IsMain).Url));

            CreateMap<PlaceVisit, VisitDto>()
                     .ForMember(d => d.DisplayName, o => o.MapFrom(s => s.Author.DisplayName))
                     .ForMember(d => d.Username, o => o.MapFrom(s => s.Author.UserName))
                     .ForMember(d => d.Image, o => o.MapFrom(s => s.Author.Photos.FirstOrDefault(x => x.IsMain).Url));
        }

    }
}