using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Places
{
    internal static class PlaceQueries
    {
        /// <summary>Places the current user may see: approved ones, plus their own and everything for admins.</summary>
        public static IQueryable<Place> VisibleTo(this IQueryable<Place> query, IUserAccessor user)
        {
            if (user.IsAdmin()) return query;
            var username = user.GetUsername();
            return query.Where(p => p.IsAproved || p.Favorites.Any(f => f.IsOwner && f.User.UserName == username));
        }

        public static IQueryable<PlaceDto> ProjectToDto(this IQueryable<Place> query, IMapper mapper, IUserAccessor user) =>
            query.ProjectTo<PlaceDto>(mapper.ConfigurationProvider, new { currentUsername = user.GetUsername() });

        public static async Task<PlaceDto> GetDtoAsync(DataContext context, IMapper mapper, IUserAccessor user, Guid id, CancellationToken ct)
        {
            var dto = await context.Places.Where(p => p.Id == id).VisibleTo(user)
                .ProjectToDto(mapper, user)
                .FirstOrDefaultAsync(ct);
            return dto?.Finish();
        }

        public static void Apply(this Place place, PlaceInput input)
        {
            place.Name = input.Name?.Trim();
            place.Type = Core.PlaceTypes.Normalize(input.Type);
            place.Description = input.Description?.Trim() ?? string.Empty;
            place.Observations = input.Observations?.Trim();
            place.Address = input.Address?.Trim();
            place.OpeningHours = input.OpeningHours?.Trim();
            place.IsAvailable = input.IsAvailable;
            place.HaveBabyChanger = input.HaveBabyChanger;
            place.IsRoomy = input.IsRoomy;
            place.IsAccessible = input.IsAccessible || place.Type == Core.PlaceTypes.Accessible;
            place.IsFree = input.IsFree;
            place.Urinals = input.Urinals;
            place.Toilets = input.Toilets;
            place.Rating = input.Rating;
            place.Lat = input.Lat;
            place.Long = input.Long;
        }
    }
}
