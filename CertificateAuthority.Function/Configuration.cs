namespace CertificateAuthority.Function
{
    internal class Configuration
    {
        public string RootCertificateId { get; set; } = "RootCertificate";
        public string StorageConnectionString { get; set; }
        public string StorageContainerName { get; set; } = "cert";
    }
}
