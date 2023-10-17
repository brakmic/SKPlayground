using System.ComponentModel;
using Microsoft.SemanticKernel;
using YamlDotNet.Serialization;

namespace SkPlayground.Plugins;

public class SecretYamlGenerator
{
    [SKFunction, Description("Generate Kubernetes Secret YAML for a self-signed certificate")]
    public string CreateSecretYaml(
    [Description("Base64 encoded certificate data")] string certData,
    [Description("Base64 encoded key data")] string keyData,
    [Description("Name of the Kubernetes Secret")] string secretName = "my-secret",
    [Description("Namespace of the Kubernetes Secret")] string secretNamespace = "default"
    )
    {
        if (string.IsNullOrWhiteSpace(certData) || string.IsNullOrWhiteSpace(certData))
        {
            throw new Exception("cert or key data invalid");
        }
        var secretObject = new
        {
            apiVersion = "v1",
            kind = "Secret",
            metadata = new
            {
                name = secretName,
                @namespace = secretNamespace
            },
            type = "kubernetes.io/tls",
            data = new
            {
                tls_crt = certData,
                tls_key = keyData
            }
        };

        var serializer = new SerializerBuilder().Build();
        var yaml = serializer.Serialize(secretObject);

        return yaml;
    }
}
