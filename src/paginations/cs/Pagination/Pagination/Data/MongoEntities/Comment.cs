using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Pagination.Data.MongoEntities;

[BsonIgnoreExtraElements]
public class Comment : MongoEntity
{
    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId UserId { get; set; }
    
    [BsonElement("message")]
    public string Message { get; set; }
}