using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;


namespace ShopDoGiaDungAPI.DTO
{
    public class LogEntry
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Timestamp")]
        public DateTime Timestamp { get; set; }

        [BsonElement("userid")]
        public string userid { get; set; }

        [BsonElement("action")]
        public string action { get; set; }

        [BsonElement("objects")]
        public string objects { get; set; }
        [BsonElement("ip")]
        public string ip { get; set; }
    }
}
