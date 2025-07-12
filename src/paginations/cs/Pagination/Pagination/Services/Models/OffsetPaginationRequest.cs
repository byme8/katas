using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Apparatus.AOT.Reflection;

namespace Pagination.Services;

[AOTReflection]
public enum OrderDirection
{
    [Description("NONE")]
    None,
    [Description("ASC")]
    Ascending,
    [Description("DESC")]
    Descending
}

public record Order<TOrderBy>(TOrderBy OrderBy, OrderDirection Direction);

public class OffsetPaginationRequest<TOrderBy>
    where TOrderBy : Enum
{
    [Required]
    public int Page { get; set; }
    
    [Required]
    public int Size { get; set; }
    
    public TOrderBy OrderBy { get; set; } = default!;
    
    public OrderDirection Direction { get; set; } = OrderDirection.Descending;
    
    public bool SkipCount { get; set; } = false;
}