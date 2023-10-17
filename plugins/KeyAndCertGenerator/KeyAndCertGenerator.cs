using System.Text;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.SemanticKernel;

namespace SkPlayground.Plugins;

public class KeyAndCertGenerator
{
    /// <summary>
    /// Generates a public-private key pair and a self-signed certificate, returning their Base64 representations concatenated into a single string.
    /// </summary>
    /// <param name="commonName">The Common Name (CN) to use for the certificate. Defaults to "localhost" if not provided.</param>
    /// <param name="delimiter">The delimiter to use for separating the private key and certificate in the returned string. Defaults to "|||" if not provided.</param>
    /// <returns>A single string containing the Base64-encoded private key and certificate, separated by the specified delimiter.</returns>
    [SKFunction, Description("Generate a public-private key pair and a self-signed certificate, returning their Base64 representations concatenated into a single string")]
    public string GenerateBase64KeyAndCert(
    [Description("The Common Name (CN) to use for the certificate")] string commonName = "localhost",
    [Description("The delimiter to use for separating the private key and certificate in the returned string")] string delimiter = "|||"

    )
    {
        using RSA rsa = RSA.Create(2048);
        CertificateRequest certReq = new CertificateRequest($"CN={commonName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        X509Certificate2 certificate = certReq.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));

        string privateKey = Convert.ToBase64String(Encoding.ASCII.GetBytes(ExportPrivateKey(rsa)));
        string cert = Convert.ToBase64String(Encoding.ASCII.GetBytes(ExportCertificate(certificate)));

        string concatStr = $"{privateKey}{delimiter}{cert}";

        return concatStr;
    }

    /// <summary>
    /// Extracts either the private key or certificate from a concatenated string.
    /// </summary>
    /// <param name="concatenatedString">The concatenated string containing the private key and certificate.</param>
    /// <param name="delimiter">The delimiter used to separate the private key and certificate in the concatenated string.</param>
    /// <param name="valueToExtract">The value to extract from the concatenated string: "key" for the private key, "cert" for the certificate.</param>
    /// <returns>The requested value, either the private key or certificate, extracted from the concatenated string.</returns>
    [SKFunction, Description("Extracts either the private key or certificate from a concatenated string")]
    public string Extract(
    [Description("The concatenated string containing the private key and certificate in the concatenated string")] string concatenatedString,
    [Description("The delimiter used to separate the private key and certificate")] string delimiter,
    [Description("The value to extract from the concatenated string: key or cert")] string valueToExtract
    )
    {
        var parts = concatenatedString.Split(new[] { delimiter }, StringSplitOptions.None);

        if ((parts.Length != 2) || string.IsNullOrEmpty(delimiter))
            throw new ArgumentException("Invalid concatenated string or delimiter.");

        if (valueToExtract.ToLower() == "key")
            return parts[0];
        else if (valueToExtract.ToLower() == "cert")
            return parts[1];
        else
            throw new ArgumentException("Invalid valueToExtract argument. Use \"key\" or \"cert\".");
    }

    /// <summary>
    /// Exports a private key to PEM format.
    /// </summary>
    /// <param name="rsa">The RSA parameters containing the private key.</param>
    /// <returns>The private key in PEM format.</returns>
    private string ExportPrivateKey(RSA rsa)
    {
        return $"-----BEGIN PRIVATE KEY-----\n{Convert.ToBase64String(rsa.ExportPkcs8PrivateKey())}\n-----END PRIVATE KEY-----";
    }

    /// <summary>
    /// Exports a certificate to PEM format.
    /// </summary>
    /// <param name="certificate">The certificate to export.</param>
    /// <returns>The certificate in PEM format.</returns>
    private string ExportCertificate(X509Certificate2 certificate)
    {
        return $"-----BEGIN CERTIFICATE-----\n{Convert.ToBase64String(certificate.RawData)}\n-----END CERTIFICATE-----";
    }
}
