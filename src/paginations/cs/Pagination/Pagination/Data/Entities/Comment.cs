namespace Pagination.Data.Entities;

public class Comment : Entity
{
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    
    // Navigation property
    public User User { get; set; } = null!;
}