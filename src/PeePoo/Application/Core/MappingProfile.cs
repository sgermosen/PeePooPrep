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
            // Supplied per query via ProjectTo(..., new { currentUsername }).
            string currentUsername = null;

            CreateMap<Photo, PhotoDto>();
            CreateMap<VisitPhoto, PhotoDto>()
                .ForMember(d => d.IsMain, o => o.Ignore());

            CreateMap<Place, PlaceDto>()
                .ForMember(d => d.OwnerUsername, o => o.MapFrom(s => s.Favorites.Where(x => x.IsOwner).Select(x => x.User.UserName).FirstOrDefault()))
                .ForMember(d => d.Image, o => o.MapFrom(s => s.Photos.OrderByDescending(x => x.IsMain).Select(x => x.Url).FirstOrDefault()))
                .ForMember(d => d.AverageRating, o => o.MapFrom(s => s.Visits.Where(v => !v.IsHidden).Average(v => (double?)v.Rating)))
                .ForMember(d => d.ReviewCount, o => o.MapFrom(s => s.Visits.Count(v => !v.IsHidden)))
                .ForMember(d => d.FavoritesCount, o => o.MapFrom(s => s.Favorites.Count(f => !f.IsOwner)))
                .ForMember(d => d.IsFavorite, o => o.MapFrom(s => s.Favorites.Any(f => f.User.UserName == currentUsername)))
                .ForMember(d => d.IsOwner, o => o.MapFrom(s => s.Favorites.Any(f => f.IsOwner && f.User.UserName == currentUsername)))
                .ForMember(d => d.DistanceKm, o => o.Ignore());

            CreateMap<Visit, VisitDto>()
                .ForMember(d => d.DisplayName, o => o.MapFrom(s => s.Author.DisplayName))
                .ForMember(d => d.Username, o => o.MapFrom(s => s.Author.UserName))
                .ForMember(d => d.PlaceName, o => o.MapFrom(s => s.Place.Name))
                .ForMember(d => d.IsMine, o => o.MapFrom(s => s.Author.UserName == currentUsername));

            CreateMap<ApplicationUser, Profiles.Profile>()
                .ForMember(d => d.Username, o => o.MapFrom(s => s.UserName))
                .ForMember(d => d.Image, o => o.Ignore())
                .ForMember(d => d.PlacesCount, o => o.MapFrom(s => s.FavoritePlaces.Count(f => f.IsOwner)))
                .ForMember(d => d.ReviewsCount, o => o.Ignore())
                .ForMember(d => d.JoinedAt, o => o.MapFrom(s => s.CreatedAt));
        }
    }
}
