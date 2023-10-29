using System.Text.Json.Serialization;

namespace SkPlayground.WebServer.Dtos;

public class HashRequest
{
    [JsonPropertyName("input")]
    public required string Input { get; set; }
}

