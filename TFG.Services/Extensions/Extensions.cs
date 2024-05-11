using TFG.Services.Pagination;

namespace TFG.Services.Extensions;

public static class Extensions
{
    public static async Task<Pagination<R>> ToPagination<T, R>(this IQueryable<T> items, int pageNumber, int pageSize,
        string orderBy, bool descending, Func<T, R> selector)
    {
        var item = await Pagination<T>.CreateAsync(items, pageNumber, pageSize, orderBy, descending);
        return new Pagination<R>(item.Items.Select(selector), item.TotalCount, pageNumber, pageSize);
    }
}