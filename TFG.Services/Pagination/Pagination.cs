using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;

namespace TFG.Services.Pagination;

public class Pagination<T>(IEnumerable<T> items, int count, int pageNumber, int pageSize)
{
    public int CurrentPage { get; private set; } = pageNumber;
    public int TotalPages { get; private set; } = (int)Math.Ceiling(count / (double)pageSize);
    public int PageSize { get; private set; } = pageSize;
    public int TotalCount { get; private set; } = count;
    public int TotalRecords { get; private set; } = count;
    public List<T> Items { get; private set; } = items.ToList();

    public static async Task<Pagination<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize,
        string orderBy, bool descending)
    {
        var count = await source.CountAsync();
        var items = await source.OrderBy(orderBy + (descending ? " descending" : "")).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        
        return new Pagination<T>(items, count, pageNumber, pageSize);
    }
}