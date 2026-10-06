using Application.Core;
using Application.Interfaces;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Places
{
    public class List
    {
        public const int DefaultLimit = 100;
        public const int MaxLimit = 500;

        public class Query : IRequest<Result<List<PlaceDto>>>
        {
            public double? Lat { get; set; }
            public double? Long { get; set; }
            public double? RadiusKm { get; set; }
            /// <summary>Free-text search over name, description and address.</summary>
            public string Search { get; set; }
            public string Type { get; set; }
            public bool? BabyChanger { get; set; }
            public bool? Roomy { get; set; }
            public bool? Accessible { get; set; }
            public bool? Free { get; set; }
            public bool? AvailableOnly { get; set; }
            /// <summary>"distance" (default when a location is given), "rating" or "recent".</summary>
            public string Sort { get; set; }
            public int? Limit { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<PlaceDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            private readonly IUserAccessor _userAccessor;

            public Handler(DataContext context, IMapper mapper, IUserAccessor userAccessor)
            {
                _mapper = mapper;
                _context = context;
                _userAccessor = userAccessor;
            }

            public async Task<Result<List<PlaceDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                IQueryable<Domain.Place> query = _context.Places.Where(p => p.IsAproved);

                var type = PlaceTypes.Normalize(request.Type);
                if (type != null)
                    query = query.Where(p => p.Type == type);
                if (request.BabyChanger == true)
                    query = query.Where(p => p.HaveBabyChanger);
                if (request.Roomy == true)
                    query = query.Where(p => p.IsRoomy);
                if (request.Accessible == true)
                    query = query.Where(p => p.IsAccessible);
                if (request.Free == true)
                    query = query.Where(p => p.IsFree);
                if (request.AvailableOnly == true)
                    query = query.Where(p => p.IsAvailable);

                if (!string.IsNullOrWhiteSpace(request.Search))
                {
                    var term = request.Search.Trim().ToLower();
                    if (term.Length > 60) term = term.Substring(0, 60);
                    query = query.Where(p => p.Name.ToLower().Contains(term)
                        || (p.Description != null && p.Description.ToLower().Contains(term))
                        || (p.Address != null && p.Address.ToLower().Contains(term)));
                }

                var hasLocation = request.Lat.HasValue && request.Long.HasValue
                    && Math.Abs(request.Lat.Value) <= 90 && Math.Abs(request.Long.Value) <= 180;
                var radius = request.RadiusKm.HasValue ? Math.Clamp(request.RadiusKm.Value, 0.1, 500) : (double?)null;
                if (hasLocation && radius.HasValue)
                {
                    var latDelta = radius.Value / 111d;
                    var cos = Math.Cos(request.Lat.Value * Math.PI / 180d);
                    var longDelta = radius.Value / (111d * Math.Max(Math.Abs(cos), 0.0001));

                    var minLat = request.Lat.Value - latDelta;
                    var maxLat = request.Lat.Value + latDelta;
                    var minLong = request.Long.Value - longDelta;
                    var maxLong = request.Long.Value + longDelta;

                    query = query.Where(p => p.Lat >= minLat && p.Lat <= maxLat && p.Long >= minLong && p.Long <= maxLong);
                }

                var limit = Math.Clamp(request.Limit ?? DefaultLimit, 1, MaxLimit);
                var sort = request.Sort?.Trim().ToLowerInvariant();

                // Distance needs every candidate in the bounding box; otherwise the database can order and cap.
                if (!hasLocation)
                {
                    query = sort == "rating"
                        ? query.OrderByDescending(p => p.Visits.Where(v => !v.IsHidden).Average(v => (double?)v.Rating) ?? p.Rating)
                        : query.OrderByDescending(p => p.CreatedAt);
                    query = query.Take(limit);
                }

                var places = (await query.ProjectToDto(_mapper, _userAccessor).ToListAsync(cancellationToken))
                    .Select(p => p.Finish())
                    .ToList();

                if (hasLocation)
                {
                    foreach (var place in places)
                        place.DistanceKm = Math.Round(GeoMath.HaversineKm(request.Lat.Value, request.Long.Value, place.Lat, place.Long), 2);

                    if (radius.HasValue)
                        places = places.Where(p => p.DistanceKm <= radius.Value).ToList();

                    places = (sort switch
                    {
                        "rating" => places.OrderByDescending(p => p.AverageRating ?? p.Rating).ThenBy(p => p.DistanceKm),
                        "recent" => places.OrderByDescending(p => p.CreatedAt),
                        _ => places.OrderBy(p => p.DistanceKm)
                    }).Take(limit).ToList();
                }

                return Result<List<PlaceDto>>.Success(places);
            }
        }
    }
}
