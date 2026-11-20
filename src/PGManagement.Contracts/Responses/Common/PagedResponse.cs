namespace PGManagement.Contracts.Responses.Common;

public class PagedResponse<T> : ApiResponse<IReadOnlyList<T>>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages { get; init; }

    public static PagedResponse<T> Create(
        IReadOnlyList<T> data,
        int page,
        int pageSize,
        int totalCount) => new()
    {
        Success = true,
        Data = data,
        Page = page,
        PageSize = pageSize,
        TotalCount = totalCount,
        TotalPages = (int)Math.Ceiling((double)totalCount / (pageSize > 0 ? pageSize : 1))
    };
}
