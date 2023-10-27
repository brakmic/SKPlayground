using System.Text.Json.Serialization;

namespace SkPlayground.WebServer.Dtos;

public class CertificateInfo
{
  [JsonPropertyName("common_name")]
  public string? CommonName { get; set; }

  [JsonPropertyName("fqdn")]
  public string? FQDN { get; set; }
}
