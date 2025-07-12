using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Taskit.Application.Common.Models;
using Taskit.Domain.Entities;

namespace Taskit.Application.Common.Mappings;

public static class MappingExtensions
{
    public static Task<PaginatedList<TDestination>> PaginatedListAsync<TDestination>(this IQueryable<TDestination> queryable, int pageNumber, int pageSize, CancellationToken cancellationToken = default) where TDestination : class
        => PaginatedList<TDestination>.CreateAsync(queryable.AsNoTracking(), pageNumber, pageSize, cancellationToken);

    public static Task<List<TDestination>> ProjectToListAsync<TDestination>(this IQueryable queryable, IConfigurationProvider configuration, CancellationToken cancellationToken = default) where TDestination : class
        => queryable.ProjectTo<TDestination>(configuration).AsNoTracking().ToListAsync(cancellationToken);

    // public static PaginatedList<TDestination> MapPaginatedList<TSource, TDestination>(this IMapper mapper, PaginatedList<TSource> source)
    //     where TSource : class
    //     where TDestination : class
    // {
    //     var items = mapper.Map<List<TDestination>>(source);
    //     return new PaginatedList<TDestination>(items, source.Count, source.PageIndex, source.TotalPages);
    // }
}
