using System.ComponentModel.DataAnnotations;

namespace Pagination.Data.Entities;

public class UserEntity : Entity
{
    public string Name { get; init; }
    
    public string Email { get; set; }
}