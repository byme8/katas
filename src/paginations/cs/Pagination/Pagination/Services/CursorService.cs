using System.Text;
using System.Text.Json;
using MessagePack;
using Pagination.Data.Entities;

namespace Pagination.Services;

public class CursorService(ILogger<CursorService> logger)
{
    public string EncodeCursor<T>(T entity, object sortValue, long id, bool isBackward = false)
    {
        var cursorData = new CursorData
        {
            Id = id,
            Value = sortValue,
            IsBackward = isBackward
        };
        var bytes = MessagePackSerializer.Serialize(cursorData);
        return Convert.ToBase64String(bytes);
    }

    public string EncodeUserCursor(User user, UserSortField orderBy, bool isBackward = false)
    {
        var sortValue = GetUserSortValue(user, orderBy);
        return EncodeCursor(user, sortValue, user.Id, isBackward);
    }

    public string EncodeFirstPageCursor(UserSortField orderBy, SortDirection direction)
    {
        // First page = null cursor, but we can encode it explicitly for consistency
        // This represents the very beginning of the dataset
        var cursorData = new CursorData
        {
            Id = 0,
            Value = GetFirstPageValue(orderBy, direction, true),
            IsBackward = false
        };

        var bytes = MessagePackSerializer.Serialize(cursorData);
        return Convert.ToBase64String(bytes);
    }

    public string EncodeLastPageCursor(UserSortField orderBy, SortDirection direction)
    {
        // Last page = first page with opposite sorting + backward flag
        // This will start from the "end" and paginate backward
        var cursorData = new CursorData
        {
            Id = 0, // Start from the beginning
            Value = GetFirstPageValue(orderBy, direction, false),
            IsBackward = true // This is the key - it's a backward cursor
        };

        var bytes = MessagePackSerializer.Serialize(cursorData);
        return Convert.ToBase64String(bytes);
    }

    private object GetFirstPageValue(UserSortField orderBy, SortDirection direction, bool isFirstPage)
    {
        if (isFirstPage)
        {
            // For first page, we want the earliest possible value in the sort direction
            return orderBy switch
            {
                UserSortField.CompanyId => direction == SortDirection.Ascending ? 0L : long.MaxValue,
                UserSortField.CreatedAt => direction == SortDirection.Ascending ? 0L : long.MaxValue,
                UserSortField.UpdatedAt => direction == SortDirection.Ascending ? 0L : long.MaxValue,
                _ => direction == SortDirection.Ascending ? "" : "zzzzz"
            };
        }
        else
        {
            // For last page, we want the "first" value in the opposite direction
            return orderBy switch
            {
                UserSortField.CompanyId => direction == SortDirection.Ascending ? long.MaxValue : 0L,
                UserSortField.CreatedAt => direction == SortDirection.Ascending ? long.MaxValue : 0L,
                UserSortField.UpdatedAt => direction == SortDirection.Ascending ? long.MaxValue : 0L,
                _ => direction == SortDirection.Ascending ? "zzzzz" : ""
            };
        }
    }


    public CursorData? DecodeCursor(string cursor)
    {
        try
        {
            var bytes = Convert.FromBase64String(cursor);
            return MessagePackSerializer.Deserialize<CursorData>(bytes);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to decode cursor: {Cursor}. Error: {Error}", cursor, ex.Message);
            
            // Try fallback to JSON decoding (for backwards compatibility)
            try
            {
                var jsonBytes = Convert.FromBase64String(cursor);
                var json = Encoding.UTF8.GetString(jsonBytes);
                var cursorData = JsonSerializer.Deserialize<CursorData>(json);
                logger.LogInformation("Successfully decoded cursor using JSON fallback");
                return cursorData;
            }
            catch (Exception jsonEx)
            {
                logger.LogWarning(jsonEx, "Failed to decode cursor using JSON fallback: {Cursor}. JSON Error: {Error}", cursor, jsonEx.Message);
                return null;
            }
        }
    }

    private object GetUserSortValue(User user, UserSortField orderBy)
    {
        return orderBy switch
        {
            UserSortField.CompanyId => user.CompanyId,
            UserSortField.Name => user.Name,
            UserSortField.Email => user.Email,
            UserSortField.PhoneNumber => user.PhoneNumber ?? string.Empty,
            UserSortField.TwitterHandle => user.TwitterHandle ?? string.Empty,
            UserSortField.FacebookProfile => user.FacebookProfile ?? string.Empty,
            UserSortField.WhatsAppNumber => user.WhatsAppNumber ?? string.Empty,
            UserSortField.InstagramHandle => user.InstagramHandle ?? string.Empty,
            UserSortField.BlueskyHandle => user.BlueskyHandle ?? string.Empty,
            UserSortField.CreatedAt => ToUnixTimestampMilliseconds(user.CreatedAt),
            UserSortField.UpdatedAt => ToUnixTimestampMilliseconds(user.UpdatedAt ?? user.CreatedAt),
            _ => ToUnixTimestampMilliseconds(user.CreatedAt)
        };
    }

    private static long ToUnixTimestampMilliseconds(DateTime dateTime)
    {
        return ((DateTimeOffset)dateTime).ToUnixTimeMilliseconds();
    }
}

[MessagePackObject]
public class CursorData
{
    [Key(0)]
    public long Id { get; set; }
    
    [Key(1)]
    public object Value { get; set; } = null!;
    
    [Key(2)]
    public bool IsBackward { get; set; } = false;
}