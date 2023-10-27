
using System.Text.Json.Serialization;

namespace SkPlayground.WebServer.Responses;

public class RsaPairResponse
{
  [JsonPropertyName("private_key")]
  public required string PrivateKey { get; set; }
  [JsonPropertyName("public_key")]
  public required string PublicKey { get; set; }
}
