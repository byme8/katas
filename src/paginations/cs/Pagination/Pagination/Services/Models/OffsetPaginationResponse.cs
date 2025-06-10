namespace Pagination.Services;

public class OffsetPaginationResponse<T>
{
    public IReadOnlyList<T> Data { get; set; }

    public int Page { get; set; }
    public int Size { get; set; }

    public long TotalCount { get; set; }
    public int TotalPages { get; set; }
}