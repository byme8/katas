namespace Pagination.Services;

public class CursorPaginationResponse<T>
{
    public required IReadOnlyList<T> Data { get; set; }
    public required CursorMetadata Metadata { get; set; }
}

public class CursorMetadata
{
    public int PageSize { get; set; }

    /// <summary>
    /// Contains available pages with their cursors.
    /// </summary>
    /// <example>
    /// [
    ///     new CursorPage("<<", "first page cursor"),
    ///     new CursorPage("...", null),
    ///     new CursorPage("<", "current page cursor - 1"),
    ///     new CursorPage("Page", "current page cursor"),
    ///     new CursorPage(">", "current page cursor + 1"),
    ///     new CursorPage("...", null),
    ///     new CursorPage(">>", "backwords page cursor for represent last page"),
    /// ]
    /// </example>
    public IReadOnlyList<CursorPage> Pages { get; set; } = []; 
}

public record CursorPage(string Page, string? Cursor);
