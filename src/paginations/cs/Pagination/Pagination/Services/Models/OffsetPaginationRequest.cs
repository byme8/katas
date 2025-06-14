using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Apparatus.AOT.Reflection;

namespace Pagination.Services;

[AOTReflection]
public enum SortDirection
{
    [Description("NONE")]
    None,
    [Description("ASC")]
    Ascending,
    [Description("DESC")]
    Descending
}

public class OffsetPaginationRequest<TOrderBy>
    where TOrderBy : Enum
{
    [Required]
    public int Page { get; set; }
    
    [Required]
    public int Size { get; set; }
    
    public TOrderBy OrderBy { get; set; }
    
    public SortDirection Direction { get; set; } = SortDirection.Descending;
    
    public bool SkipCount { get; set; } = false;
}