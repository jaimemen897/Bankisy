using TFG.Services.Pagination;

namespace TFG.Services.Extensions;

public static class Extensions
{
    public static async Task<Pagination<TR>> ToPagination<T, TR>(this IQueryable<T> items, int pageNumber, int pageSize,
        string orderBy, bool descending, Func<T, TR> selector)
    {
        var item = await Pagination<T>.CreateAsync(items, pageNumber, pageSize, orderBy, descending);
        return new Pagination<TR>(item.Items.Select(selector), item.TotalCount, pageNumber, pageSize);
    }
}