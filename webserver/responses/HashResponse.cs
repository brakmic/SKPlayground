namespace SkPlayground.WebServer.Responses;

using System.Text.Json.Serialization;

public class HashResponse
{
  [JsonPropertyName("value")]
  public required string Value { get; set; }
}
