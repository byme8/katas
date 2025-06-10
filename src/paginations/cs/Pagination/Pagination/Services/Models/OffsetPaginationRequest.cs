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

public class OffsetPaginationRequest<TOrderBy, TOrder>
    where TOrderBy : Enum
    where TOrder : Enum
{
    [Required]
    public int Page { get; set; }
    
    [Required]
    public int Size { get; set; }
    
    [Required]
    public Sorting<TOrderBy, TOrder>[]? Ordering { get; set; }
}

public class Sorting<TOrderBy, TOrder>
{
    [Required]
    public TOrderBy OrderBy { get; set; }
    [Required]
    public TOrder Direction { get; set; }
}