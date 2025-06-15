using System.ComponentModel.DataAnnotations;
using Apparatus.AOT.Reflection;

namespace Pagination.Services;

public class CursorPaginationRequest<TOrderBy>
    where TOrderBy : Enum
{
    [Required]
    public int Size { get; set; }
    
    public string? Cursor { get; set; }
    
    public TOrderBy OrderBy { get; set; } = default!;
    
    public SortDirection Direction { get; set; } = SortDirection.Descending;
    
    public bool Backward { get; set; } = false;
}