using System.Text.Json.Serialization;
using Anthropic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyDemoAPI.Data.Models;
public class Conversation
{
    // [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
    // [BsonRepresentation(BsonType.ObjectId)]
    // [BsonSerializer(typeof(StringSerializer))]
    // [BsonId]
    // [BsonRepresentation(BsonType.ObjectId)]
    [JsonPropertyName("_id")]
    public string? Id { get; set; }

    // [JsonPropertyName("__created")]
    // public DateTime Created { get; init; } = DateTime.UtcNow;

    [BsonIgnore]
    [JsonPropertyName("__created")]
    public DateTime? Created {
        get {
            return Id is not null ? new ObjectId(Id).CreationTime : null;
        }
    }

    // public DateTime Created { get; init; } = DateTime.Now;
    
    // [BsonElement("title")]
    public string? Title { get; set; }
    public ICollection<MessageInfo> Messages { get; set; } = new List<MessageInfo>();

}

