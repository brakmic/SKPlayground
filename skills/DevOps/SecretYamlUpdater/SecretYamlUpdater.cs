using System.ComponentModel;
using System.Text.RegularExpressions;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace SkPlayground.Plugins;

public class SecretYamlUpdater
{
    /// <summary>
    /// Updates a Kubernetes Secret YAML string with the provided base64-encoded key and certificate.
    /// </summary>
    /// <param name="yamlContent">The YAML content as a string.</param>
    /// <param name="base64Key">The base64-encoded private key.</param>
    /// <param name="base64Cert">The base64-encoded certificate.</param>
    /// <returns>The updated YAML content.</returns>
    [SKFunction, Description("Updates a Kubernetes Secret YAML string with the provided base64-encoded key and certificate.")]
    [SKParameter("yamlContent", "YAML content as string")]
    [SKParameter("base64Key", "The base64-encoded private key")]
    [SKParameter("base64Cert", "The base64-encoded certificate")]
    public string UpdateKubernetesSecretYamlString(SKContext context)
    {
        string yamlContent = context.Variables["yamlContent"];
        string base64Key = context.Variables["base64Key"];
        string base64Cert = context.Variables["base64Cert"];
        return UpdateKubernetesSecretYamlContent(yamlContent, base64Key, base64Cert);
    }

    /// <summary>
    /// Search and replace *.key and *.cert entries in a Secret YAML
    /// </summary>
    /// <param name="yamlContent">YAML content</param>
    /// <param name="base64Key">The base64-encoded private key</param>
    /// <param name="base64Cert">The base64-encoded certificate</param>
    /// <returns>Updated Secret YAML</returns>
    /// <exception cref="InvalidOperationException"></exception>
    private string UpdateKubernetesSecretYamlContent(string yamlContent, string base64Key, string base64Cert)
    {
        Console.WriteLine($"\nINPUT: {yamlContent}\n");

        // Define a regex pattern to isolate the data: section
        var dataSectionRegex = new Regex(@"\bdata:(?:[^\n]*\n)((?:[ \t].*\n?)*)", RegexOptions.Compiled);

        // Extract the data: section
        var match = dataSectionRegex.Match(yamlContent);
        if (match.Success)
        {
            string dataSection = match.Groups[1].Value;

            // Define a regex pattern to match the key and cert fields within the data: section
            var fieldRegex = new Regex(@"([ \t]*)(.*?)(key|cert)(.*?:[ \t]*).*?$", RegexOptions.Multiline | RegexOptions.Compiled);

            // Replace the fields within the data: section
            string updatedDataSection = fieldRegex.Replace(dataSection, fieldMatch =>
            {
                string field = fieldMatch.Groups[3].Value;
                if (field.Contains("key"))
                {
                    return $"{fieldMatch.Groups[1].Value}{fieldMatch.Groups[2].Value}key{fieldMatch.Groups[4].Value} {base64Key}";
                }
                else // field contains "cert"
                {
                    return $"{fieldMatch.Groups[1].Value}{fieldMatch.Groups[2].Value}cert{fieldMatch.Groups[4].Value} {base64Cert}";
                }
            });

            // Replace the original data: section with the updated data: section
            string updatedYamlContent = dataSectionRegex.Replace(yamlContent, $"data:\n{updatedDataSection}");
            return updatedYamlContent;
        }
        else
        {
            return yamlContent;
        }
    }
}

