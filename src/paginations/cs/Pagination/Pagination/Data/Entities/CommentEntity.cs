using System.ComponentModel.DataAnnotations.Schema;

namespace Pagination.Data.Entities;

public class CommentEntity : Entity
{
    [ForeignKey(nameof(UserEntity.Id))]
    public int UserId { get; init; }
    
    public string Message { get; init; }
}