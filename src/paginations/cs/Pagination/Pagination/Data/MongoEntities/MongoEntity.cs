using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Pagination.Data.MongoEntities;

public abstract class MongoEntity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId Id { get; set; }

    [BsonElement("createdAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updatedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? UpdatedAt { get; set; }
    
    [BsonElement("deletedAt")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime? DeletedAt { get; set; }

    [BsonElement("isDeleted")]
    public bool IsDeleted { get; set; }
}