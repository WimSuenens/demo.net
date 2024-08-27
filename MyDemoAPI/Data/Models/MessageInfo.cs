using System.Text.Json.Serialization;
using Anthropic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyDemoAPI.Data.Models;

public class MessageInfo
{
  [JsonPropertyName("_id")]
  public Guid Id { get; set; } = Guid.NewGuid();
  public required Message Message { get; init; }

  [JsonPropertyName("__created")]
  public DateTime Created { get; init; } = DateTime.UtcNow;

  // public List<File> files { get; set; }

}
