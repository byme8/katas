namespace Pagination.Services;

public class CursorPaginationResponse<T>
{
    public required IReadOnlyList<T> Data { get; set; }
    public required CursorMetadata Metadata { get; set; }
}

public class CursorMetadata
{
    public int PageSize { get; set; }
    public string? NextCursor { get; set; }
    public string? PreviousCursor { get; set; }
    public string? FirstPageCursor { get; set; }
    public string? LastPageCursor { get; set; }
    
    public bool HasNextPage => !string.IsNullOrEmpty(NextCursor);
    public bool HasPreviousPage => !string.IsNullOrEmpty(PreviousCursor);
}