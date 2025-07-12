using System.ComponentModel.DataAnnotations;
using Apparatus.AOT.Reflection;

namespace Pagination.Services;

public class CursorPaginationRequest<TOrderBy>
    where TOrderBy : Enum
{
    [Required]
    public int Size { get; set; }
    
    public string? Cursor { get; set; }

    public Order<TOrderBy>? Order { get; set; }
}