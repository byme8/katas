using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Pagination.Data.Entities;

[Index(nameof(DeletedAt), nameof(IsDeleted))]
public class Entity
{
    [Key]
    public int Id { get; init; }

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? UpdatedAt { get; set; }
    
    public DateTimeOffset? DeletedAt { get; set; }

    public bool IsDeleted { get; set; }
}