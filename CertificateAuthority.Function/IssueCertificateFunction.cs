using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Security.Cryptography.X509Certificates;

namespace CertificateAuthority.Function
{
    public static class IssueCertificateFunction
    {
        [FunctionName("IssueCertificate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "issueCertificate")]
            HttpRequest req, ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var certificateIssuer = CertificateFunctionHelper.CreateCertificateIssuer(Environment.CurrentDirectory);
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            var (subjectName, publicKey) = ExtractData(requestBody);
            var certificate = await certificateIssuer.IssueCertificateAsync(subjectName, publicKey);

            byte[] certificateBuffer = certificate.Export(X509ContentType.Cert);
            string encodedCertificate = Convert.ToBase64String(certificateBuffer);
            return new OkObjectResult(new { certificate = encodedCertificate });
        }

        private static (string subjectName, RSAPublicKeyParameters publicKey) ExtractData(string requestBody)
        {
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string subjectName = data.subjectName;
            byte[] exponent = Convert.FromBase64String((string)data.publicKey.exponent);
            byte[] modulus = Convert.FromBase64String((string)data.publicKey.modulus);
            var publicKey = new RSAPublicKeyParameters(exponent, modulus);
            return (subjectName, publicKey);
        }
    }
}
