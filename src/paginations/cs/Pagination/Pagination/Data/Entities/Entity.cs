using NodaTime;

namespace Pagination.Data.Entities;

public abstract class Entity
{
    public long Id { get; set; }
    public Instant CreatedAt { get; set; }
    public Instant? UpdatedAt { get; set; }
    public Instant? DeletedAt { get; set; }
    public bool IsDeleted { get; set; }
}