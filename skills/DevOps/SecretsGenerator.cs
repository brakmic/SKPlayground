using System.ComponentModel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using YamlDotNet.Serialization;

namespace Plugins
{
    public class SecretYamlGenerator
    {
        [SKFunction, Description("Generate Kubernetes Secret YAML for a self-signed certificate")]
        [SKParameter("certData", "Base64 encoded certificate data")]
        [SKParameter("keyData", "Base64 encoded key data")]
        [SKParameter("secretName", "Name of the Kubernetes Secret")]
        [SKParameter("secretNamespace", "Namespace of the Kubernetes Secret")]
        public string CreateSecretYaml(SKContext context)
        {
            var certData = context.Variables["certData"];
            var keyData = context.Variables["keyData"];

            if (string.IsNullOrWhiteSpace(certData) || string.IsNullOrWhiteSpace(certData))
            {
                throw new Exception("cert or key data invalid");
            }

            var secretName = context.Variables["secretName"] ?? "my-secret";
            var secretNamespace = context.Variables["secretNamespace"] ?? "default";
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
}