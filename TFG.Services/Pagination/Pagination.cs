using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;

namespace TFG.Services.Pagination;

public class Pagination<T> : List<T>
{
    public int CurrentPage { get; private set; }
    public int TotalPages { get; private set; }
    public int PageSize { get; private set; }
    public int TotalCount { get; private set; }

    public Pagination(IEnumerable<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        this.AddRange(items);
    }

    public static async Task<Pagination<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize,
        string orderBy, bool descending)
    {
        var count = await source.CountAsync();
        var items = await source.OrderBy(orderBy + (descending ? " descending" : "")).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        
        return new Pagination<T>(items, count, pageNumber, pageSize);
    }
}