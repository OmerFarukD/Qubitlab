namespace Qubitlab.Abstractions.Pagination;

public interface IPageResponse<T>
{
    IReadOnlyList<T> Items { get; }
    int TotalCount { get; }
    int PageIndex { get; }
    int PageSize { get; }
    int TotalPages { get; }
    bool HasNextPage { get; }
    bool HasPreviousPage { get; }
}