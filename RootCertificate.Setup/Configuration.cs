namespace RootCertificate.Setup
{
    internal class Configuration
    {
        public string ClientId { get; set; }
        public string TenantId { get; set; }
        public string VaultName { get; set; }
        public string RootCertificateName { get; set; } = default!;
    }
}
