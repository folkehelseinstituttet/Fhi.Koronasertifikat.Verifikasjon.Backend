namespace FHICORC.Application.Models.Options
{
    public class SecurityOptions
    {
        public bool CheckApiKeyHeader { get; set; }
        public string ApiKeyHeader { get; set; }
        public string ApiKey { get; set; }
        public string PublicKeyCertificatePath { get; set; }
    }
}
