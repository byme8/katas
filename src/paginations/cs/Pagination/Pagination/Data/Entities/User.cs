namespace Pagination.Data.Entities;

public class User : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // Navigation property
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}