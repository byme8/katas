using Microsoft.EntityFrameworkCore;

namespace TinyUrl.Data.Tables;

[Index(nameof(LongUrl)), Index(nameof(EncodedId))]
public class UrlEntity : Entity
{
    public string LongUrl { get; set; }
    
    public string? EncodedId { get; set; }
}