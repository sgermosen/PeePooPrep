using Application.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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
        public class Query : IRequest<Result<List<PlaceDto>>>
        {
            public double? Lat { get; set; }
            public double? Long { get; set; }
            public double? RadiusKm { get; set; }
            public string Type { get; set; }
            public bool? BabyChanger { get; set; }
            public bool? Roomy { get; set; }
            public bool? AvailableOnly { get; set; }
        }

        public class Handler : IRequestHandler<Query, Result<List<PlaceDto>>>
        {
            private readonly DataContext _context;
            private readonly IMapper _mapper;
            public Handler(DataContext context, IMapper mapper)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<Result<List<PlaceDto>>> Handle(Query request, CancellationToken cancellationToken)
            {
                IQueryable<Domain.Place> query = _context.Places;

                if (!string.IsNullOrWhiteSpace(request.Type))
                    query = query.Where(p => p.Type == request.Type);
                if (request.BabyChanger == true)
                    query = query.Where(p => p.HaveBabyChanger);
                if (request.Roomy == true)
                    query = query.Where(p => p.IsRoomy);
                if (request.AvailableOnly == true)
                    query = query.Where(p => p.IsAvailable);

                var hasLocation = request.Lat.HasValue && request.Long.HasValue;
                if (hasLocation && request.RadiusKm.HasValue)
                {
                    var radius = request.RadiusKm.Value;
                    var latDelta = radius / 111d;
                    var cos = Math.Cos(request.Lat.Value * Math.PI / 180d);
                    var longDelta = radius / (111d * Math.Max(Math.Abs(cos), 0.0001));

                    var minLat = request.Lat.Value - latDelta;
                    var maxLat = request.Lat.Value + latDelta;
                    var minLong = request.Long.Value - longDelta;
                    var maxLong = request.Long.Value + longDelta;

                    query = query.Where(p => p.Lat >= minLat && p.Lat <= maxLat && p.Long >= minLong && p.Long <= maxLong);
                }

                var places = await query
                    .ProjectTo<PlaceDto>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                if (hasLocation)
                {
                    foreach (var place in places)
                        place.DistanceKm = Math.Round(Haversine(request.Lat.Value, request.Long.Value, place.Lat, place.Long), 2);

                    if (request.RadiusKm.HasValue)
                        places = places.Where(p => p.DistanceKm <= request.RadiusKm.Value).ToList();

                    places = places.OrderBy(p => p.DistanceKm).ToList();
                }
                else
                {
                    places = places.OrderByDescending(p => p.CreatedAt).ToList();
                }

                return Result<List<PlaceDto>>.Success(places);
            }

            private static double Haversine(double lat1, double lon1, double lat2, double lon2)
            {
                const double earthRadiusKm = 6371d;
                var dLat = (lat2 - lat1) * Math.PI / 180d;
                var dLon = (lon2 - lon1) * Math.PI / 180d;
                var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                        Math.Cos(lat1 * Math.PI / 180d) * Math.Cos(lat2 * Math.PI / 180d) *
                        Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
                return earthRadiusKm * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            }
        }
    }
}
