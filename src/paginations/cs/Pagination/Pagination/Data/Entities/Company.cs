namespace Pagination.Data.Entities;

public class Company : Entity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    // Navigation property
    public ICollection<User> Users { get; set; } = new List<User>();
}