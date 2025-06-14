namespace Pagination.Services;

public class OffsetPaginationResponse<T>
{
    public required IReadOnlyList<T> Data { get; set; }
    public required PaginationMetadata Metadata { get; set; }
}

public class PaginationMetadata
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public long? TotalCount { get; set; }
    public int? TotalPages { get; set; }
    
    public bool CountSkipped => !TotalCount.HasValue;
    
    public bool HasPreviousPage => CurrentPage > 1;
    public bool? HasNextPage => CountSkipped ? null : CurrentPage < TotalPages;
    
    public int? PreviousPage => HasPreviousPage ? CurrentPage - 1 : null;
    public int? NextPage => CountSkipped ? CurrentPage + 1 : (HasNextPage == true ? CurrentPage + 1 : null);
    
    public int FirstPage => 1;
    public int? LastPage => CountSkipped ? null : TotalPages;
    
    public long? ItemsFrom => CountSkipped ? null : (TotalCount > 0 ? ((CurrentPage - 1) * PageSize) + 1 : 0);
    public long? ItemsTo => CountSkipped ? null : Math.Min(CurrentPage * PageSize, TotalCount!.Value);
}