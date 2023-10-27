namespace SkPlayground.WebServer.Responses;

using System.Text.Json.Serialization;

public class KeyCertResponse
{
  [JsonPropertyName("private_key")]
  public required string PrivateKey { get; set; }

  [JsonPropertyName("certificate")]
  public required string Certificate { get; set; }
}
