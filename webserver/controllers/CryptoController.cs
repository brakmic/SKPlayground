using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Cryptography;
using SkPlayground.WebServer.Responses;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SkPlayground.WebServer.Dtos;

namespace SKPlayground.webserver.controllers
{
    [ApiController]
    [Route("/")]
    public class CryptoController : ControllerBase
    {

        [HttpPost("GenerateSHA256Hash")]
        [SwaggerOperation(
            Summary = "Generates an SHA256 Hash of the provided input string.",
            OperationId = "GenerateSHA256Hash",
            Description = "Generates an SHA256 hash of the provided input string."
        )]
        [Consumes("application/json", "text/plain")]
        [Produces("application/json")]
        [SwaggerResponse(200, "SHA256 Hash generated successfully.", typeof(HashResponse), Description = "SHA256 Hash generated successfully.")]
        public IActionResult GenerateSha256Hash([FromBody] HashRequest hashRequest)
        {
            if (hashRequest == null || string.IsNullOrEmpty(hashRequest.Input))
            {
                return BadRequest("Input string is null or empty.");
            }

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(hashRequest.Input);
            var hashBytes = sha256.ComputeHash(bytes);
            var hashString = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

            return Ok(new HashResponse { Value = hashString });
        }

        [HttpPost("GenerateMD5Hash")]
        [SwaggerOperation(
            Summary = "Generates an MD5 Hash of the provided input string.",
            OperationId = "GenerateMD5Hash",
            Description = "Generates an MD5 hash of the provided input string. Note: MD5 is considered weak and not recommended for cryptographic purposes."
        )]
        [Produces("application/json")]
        [SwaggerResponse(200, "MD5 Hash generated successfully.", typeof(HashResponse), Description = "MD5 Hash generated successfully.")]
        public IActionResult GenerateMD5Hash([FromBody] HashRequest hashRequest)
        {
            if (hashRequest == null || string.IsNullOrEmpty(hashRequest.Input))
            {
                return BadRequest("Input string is null or empty.");
            }

            using var md5 = MD5.Create();
            var bytes = Encoding.UTF8.GetBytes(hashRequest.Input);
            var hashBytes = md5.ComputeHash(bytes);
            var hashString = BitConverter.ToString(hashBytes).Replace("-", string.Empty);

            return Ok(new HashResponse { Value = hashString });
        }

        [HttpGet("GenerateRSAPair")]
        [SwaggerOperation(
            Summary = "Generates an RSA key pair (public and private keys).",
            OperationId = "GenerateRSAPair",
            Description = "Generates an RSA key pair encoded in base64 format."
        )]
        [Produces("application/json")]
        [SwaggerResponse(200, "RSA key pair generated successfully.", typeof(RsaPairResponse))]
        public ActionResult<RsaPairResponse> GenerateRSAPair()
        {
            using (RSA rsa = RSA.Create(2048))
            {
                var privateKey = rsa.ExportRSAPrivateKey();
                var privateKeyBase64 = Convert.ToBase64String(privateKey);

                var publicKey = rsa.ExportRSAPublicKey();
                var publicKeyBase64 = Convert.ToBase64String(publicKey);

                return Ok(new RsaPairResponse
                {
                    PrivateKey = privateKeyBase64,
                    PublicKey = publicKeyBase64
                });
            }
        }



        [HttpGet("GenerateSelfSignedKeyCert")]
        [SwaggerOperation(
            Summary = "Generates self-signed key and certificate encoded in base64 format.",
            OperationId = "GenerateSelfSignedKeyCert",
            Description = "Generates self-signed key and certificate encoded in base64 format."
        )]
        [Produces("application/json")]
        [SwaggerResponse(200, "Key and certificate generated successfully.", typeof(KeyCertResponse))]
        public ActionResult<KeyCertResponse> GenerateKeyCert()
        {
            using (RSA rsa = RSA.Create(2048))
            {
                // Generate private key
                var privateKey = rsa.ExportRSAPrivateKey();
                var privateKeyBase64 = Convert.ToBase64String(privateKey);

                // Generate self-signed certificate
                var certificateRequest = new CertificateRequest(
                    "CN=SelfSignedCertificate",
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                var certificate = certificateRequest.CreateSelfSigned(
                    DateTimeOffset.UtcNow,
                    DateTimeOffset.UtcNow.AddYears(1));

                var certBytes = certificate.Export(X509ContentType.Cert);
                var certBase64 = Convert.ToBase64String(certBytes);

                return Ok(new KeyCertResponse
                {
                    PrivateKey = privateKeyBase64,
                    Certificate = certBase64
                });
            }
        }

        [HttpPost("GenerateCustomKeyCert")]
        [SwaggerOperation(
            Summary = "Creates self-signed key and certificate based on user-provided information. It is encoded in base64 format.",
            OperationId = "GenerateCustomKeyCert",
            Description = "Creates self-signed key and certificate based on user-provided information."
        )]
        [Produces("application/json")]
        [SwaggerResponse(200, "Certificate created successfully.", typeof(KeyCertResponse))]
        public ActionResult<KeyCertResponse> GenerateCustomKeyCert([FromBody] CertificateInfo certInfo)
        {
            using (RSA rsa = RSA.Create(2048))
            {
                var privateKey = rsa.ExportRSAPrivateKey();
                var privateKeyBase64 = Convert.ToBase64String(privateKey);

                var subject = new X500DistinguishedName($"CN={certInfo.CommonName}");
                var certificateRequest = new CertificateRequest(
                    subject,
                    rsa,
                    HashAlgorithmName.SHA256,
                    RSASignaturePadding.Pkcs1);

                // If FQDN is provided, add it as a Subject Alternative Name
                if (!string.IsNullOrEmpty(certInfo.FQDN))
                {
                    var sanBuilder = new SubjectAlternativeNameBuilder();
                    sanBuilder.AddDnsName(certInfo.FQDN);
                    certificateRequest.CertificateExtensions.Add(sanBuilder.Build());
                }

                var certificate = certificateRequest.CreateSelfSigned(
                    DateTimeOffset.UtcNow,
                    DateTimeOffset.UtcNow.AddYears(1));

                var certBytes = certificate.Export(X509ContentType.Cert);
                var certBase64 = Convert.ToBase64String(certBytes);

                return Ok(new KeyCertResponse
                {
                    PrivateKey = privateKeyBase64,
                    Certificate = certBase64
                });
            }
        }

    }
}