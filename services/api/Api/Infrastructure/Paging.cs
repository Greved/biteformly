using Microsoft.EntityFrameworkCore;

namespace BiteForm.Api.Infrastructure;

public static class Paging
{
    public static (int page, int pageSize) Normalize(int page, int pageSize, int maxPageSize = 100)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > maxPageSize) pageSize = maxPageSize;
        return (page, pageSize);
    }

    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int page, int pageSize, CancellationToken ct)
    {
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<T>(items, total, page, pageSize);
    }
}

public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

