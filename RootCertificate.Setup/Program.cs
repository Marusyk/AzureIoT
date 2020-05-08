using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace RootCertificate.Setup
{
    class Program
    {
        static async Task Main()
        {
            var configuration = new Configuration
            {
                ClientId = "", // EdgeDevice.RequestCertificate
                TenantId = "",
                VaultName = "iot-pki",
                RootCertificateName = "RootCertificate"
            };

            var acquireTokenResult = await new AuthenticationHelper(configuration.ClientId, configuration.TenantId, AuthenticationHelper.KeyVaultScopes)
                .AcquireTokenAsync();

            var certificate = await new RootCertificateHelper(configuration.VaultName, configuration.RootCertificateName, acquireTokenResult.AccessToken)
                .GenerateRootCertificate();

            await WriteCertificateToFile(certificate);
            Console.ReadKey();
        }

        private static async Task WriteCertificateToFile(X509Certificate2 certificate)
        {
            const string fileName = "RootCert.cer";
            var fullFilePath = Path.Combine(Environment.CurrentDirectory, fileName);
            await File.WriteAllTextAsync(fullFilePath, Convert.ToBase64String(certificate.Export(X509ContentType.Cert)));
            Console.WriteLine($"Stored public issuer certificate at '{fullFilePath}'");
        }
    }
}
