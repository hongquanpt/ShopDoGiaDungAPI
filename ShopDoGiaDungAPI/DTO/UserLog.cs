using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

public class UserLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonElement("userId")]
    public string UserId { get; set; }

    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [BsonElement("action")]
    public string Action { get; set; }

    [BsonElement("details")]
    public object Details { get; set; }
}
