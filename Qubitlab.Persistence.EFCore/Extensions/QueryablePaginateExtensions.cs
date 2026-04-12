using Microsoft.EntityFrameworkCore;
using Qubitlab.Persistence.EFCore.Entities;

namespace Qubitlab.Persistence.EFCore.Extensions;

public static class QueryablePaginateExtensions
{
    public static async Task<Paginate<T>> ToPaginateAsync<T>(
        this IQueryable<T> source,
        int index,
        int size,
        CancellationToken cancellationToken = default
    )
    {

        int count = await source.CountAsync(cancellationToken);

        List<T> items = await source
            .Skip(index * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return new Paginate<T>
        {
            Index = index,
            Count = count,
            Items = items,
            Size = size,
            Pages = (int)Math.Ceiling(count / (double)size)
        };
    }

    public static Paginate<T> ToPaginate<T>(this IQueryable<T> source, int index, int size)
    {
        int count = source.Count();
        var items = source.Skip(index * size).Take(size).ToList();

        Paginate<T> list = new()
        {
            Index = index,
            Size = size,
            Count = count,
            Items = items,
            Pages = (int)Math.Ceiling(count / (double)size)
        };
        
        return list;
    }


    public static async Task<Paginate<T>> ToPaginateWithSingleQueryAsync<T>(
        this IQueryable<T> source,
        int index,
        int size,
        CancellationToken cancellationToken = default
    )
    {

        var items = await source
            .Skip(index * size)
            .Take(size + 1)
            .ToListAsync(cancellationToken);

        bool hasMore = items.Count > size;
        if (hasMore)
        {
            items.RemoveAt(items.Count - 1);
        }

   
        int count;
        if (!hasMore && index == 0)
        {
            count = items.Count;
        }
        else if (!hasMore)
        {
            count = index * size + items.Count;
        }
        else
        {
   
            count = await source.CountAsync(cancellationToken);
        }

        return new Paginate<T>
        {
            Index = index,
            Count = count,
            Items = items,
            Size = size,
            Pages = (int)Math.Ceiling(count / (double)size)
        };
    }


}