namespace Pagination.Data.Entities;

public class User : Entity
{
    public long CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? TwitterHandle { get; set; }
    public string? FacebookProfile { get; set; }
    public string? WhatsAppNumber { get; set; }
    public string? InstagramHandle { get; set; }
    public string? BlueskyHandle { get; set; }
    
    // Navigation property
    public Company Company { get; set; } = null!;
}