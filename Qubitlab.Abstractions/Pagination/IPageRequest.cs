namespace Qubitlab.Abstractions.Pagination;

public interface IPageRequest
{
    int PageIndex { get; }
    int PageSize { get; }
    string? OrderBy { get; }
    bool IsDescending { get; }
}