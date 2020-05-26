﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDevice.RequestCertificate
{
    internal class Program
    {
        internal static async Task Main()
        {
            SetDefaultSerializerSettings();
            var configuration = ReadConfiguration();
            var subjectName = ReadAndConfirmSubjectName(configuration);

            var key = RSA.Create();
            var publicParameters = key.ExportParameters(false);
            var certificate = await IssueCertificate(subjectName, publicParameters, configuration);
            await WriteToFileAsync(certificate, "IssuedCertificate.cer");
            var certificateWithPrivateKey = CreateCertificateWithPrivateKey(certificate, key);
            StoreCertificateInUserStore(certificateWithPrivateKey);

            Console.WriteLine("Stored issued certificate in the certificate store:");
            Console.WriteLine(certificateWithPrivateKey);
            Console.ReadKey();
        }

        private static void SetDefaultSerializerSettings()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        private static Configuration ReadConfiguration()
        {
            var configurationRoot = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
            var configuration = configurationRoot.Get<Configuration>();
            return configuration;
        }

        private static string ReadAndConfirmSubjectName(Configuration configuration)
        {
            Console.Write($"The device name is '{configuration.DeviceName}'. Please confirm (Y/n): ");
            var confirmation = Console.ReadLine();
            switch (confirmation)
            {
                case "y":
                case "Y":
                    return configuration.DeviceName;
                case "n":
                case "N":
                    Console.WriteLine("Enter the desired subject name:");
                    return Console.ReadLine();
                default:
                    return ReadAndConfirmSubjectName(configuration);
            }
        }

        private static async Task<X509Certificate2> IssueCertificate(string subjectName, RSAParameters publicParameters, Configuration configuration)
        {
            using (var client = new HttpClient { BaseAddress = new Uri(configuration.FunctionBaseUrl) })
            {
                var request = new IssueCertificateRequest(subjectName, publicParameters);
                var httpContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, MediaTypeNames.Application.Json);

                using (var responseMessage = await client.PostAsync("api/issueCertificate", httpContent))
                {
                    responseMessage.EnsureSuccessStatusCode();
                    var serializedResponse = await responseMessage.Content.ReadAsStringAsync();

                    var response = JsonConvert.DeserializeObject<IssueCertificateResponse>(serializedResponse) ??
                        throw new Exception("Unexpected response");

                    var certificate = new X509Certificate2(Convert.FromBase64String(response.Certificate));
                    return certificate;
                }
            }
        }

        private static async Task WriteToFileAsync(X509Certificate2 certificate, string fileName)
        {
            var fullPath = Path.Combine(Environment.CurrentDirectory, fileName);
            await File.WriteAllTextAsync(fullPath, Convert.ToBase64String(certificate.Export(X509ContentType.Cert)));
            Console.WriteLine($"Certificate was stored in file '{fullPath}'");
        }

        private static X509Certificate2 CreateCertificateWithPrivateKey(X509Certificate2 certificate, RSA key)
        {
            var certificateWithPrivateKey = certificate.CopyWithPrivateKey(key);
            var rawCertificate = certificateWithPrivateKey.Export(X509ContentType.Pfx);
            var persistableCertificate = new X509Certificate2(rawCertificate, string.Empty, X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.UserKeySet);
            return persistableCertificate;
        }

        private static void StoreCertificateInUserStore(X509Certificate2 certificateWithPrivateKey)
        {
            using var store = new X509Store(StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            DeleteOldCertificatesIfFound(store, certificateWithPrivateKey);
            store.Add(certificateWithPrivateKey);
        }

        private static void DeleteOldCertificatesIfFound(X509Store store, X509Certificate2 newCertificate)
        {
            var subject = newCertificate.GetNameInfo(X509NameType.SimpleName, false);
            if (subject == null)
                return;

            var certificateCollection = store.Certificates.Find(X509FindType.FindBySubjectName, subject, false);
            store.RemoveRange(certificateCollection);
        }
    }
}
