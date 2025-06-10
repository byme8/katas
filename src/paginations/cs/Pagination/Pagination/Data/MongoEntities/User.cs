using MongoDB.Bson.Serialization.Attributes;

namespace Pagination.Data.MongoEntities;

[BsonIgnoreExtraElements]
public class User : MongoEntity
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;
    
    [BsonElement("email")]
    public string Email { get; set; } = string.Empty;
}