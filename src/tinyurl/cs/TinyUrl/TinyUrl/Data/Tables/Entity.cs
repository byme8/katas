
namespace TinyUrl.Data.Tables;

public class Entity
{
    public int Id { get; set; }

    public DateTimeOffset CreateDate { get; set; }

    public DateTimeOffset? UpdateTime { get; set; }

    public DateTimeOffset? DeleteDate { get; set; }
}