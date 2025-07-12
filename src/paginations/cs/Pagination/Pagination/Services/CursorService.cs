using System.Text;
using System.Text.Json;
using MessagePack;
using Microsoft.EntityFrameworkCore;
using Pagination.Data.Entities;

namespace Pagination.Services;

public class CursorService(ILogger<CursorService> logger)
{
    public string EncodeCursor<TCursorColumn, TCursorColumnValue>(TCursorColumn orderBy, TCursorColumnValue orderValue, OrderDirection direction, long id)
    {
        var cursorData = new CursorData<TCursorColumn>
        {
            Id = id,
            OrderBy = orderBy,
            Value = orderValue!,
            Direction = direction
        };

        var bytes = MessagePackSerializer.Serialize(cursorData);
        return Convert.ToBase64String(bytes);
    }

    public CursorData<TCursorColumn>? DecodeCursor<TCursorColumn>(string? cursor)
    {
        if (string.IsNullOrEmpty(cursor))
        {
            return null;
        }
        
        try
        {
            var bytes = Convert.FromBase64String(cursor);
            return MessagePackSerializer.Deserialize<CursorData<TCursorColumn>>(bytes);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to decode cursor: {Cursor}. Error: {Error}", cursor, ex.Message);
            return null;
        }
    }
}

[MessagePackObject]
public class CursorData<TColumn>
{
    [Key(0)]
    public long Id { get; set; }

    [Key(1)]
    public TColumn OrderBy { get; set; }

    [Key(2)]
    public object Value { get; set; }

    [Key(3)] 
    public OrderDirection Direction { get; set; }
}