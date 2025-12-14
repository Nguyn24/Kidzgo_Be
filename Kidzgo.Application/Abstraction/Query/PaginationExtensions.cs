namespace Kidzgo.Application.Abstraction.Query;

public static class PaginationExtensions
{
    public static IQueryable<T> ApplyPagination<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    {
        return query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
}