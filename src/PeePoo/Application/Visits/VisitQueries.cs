using Application.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Domain;
using System.Linq;

namespace Application.Visits
{
    internal static class VisitQueries
    {
        public static IQueryable<VisitDto> ProjectToDto(this IQueryable<Visit> query, IMapper mapper, IUserAccessor user) =>
            query.ProjectTo<VisitDto>(mapper.ConfigurationProvider, new { currentUsername = user.GetUsername() });
    }
}
