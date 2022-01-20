using System.Text.Json.Serialization;

namespace FHICORC.Application.Models.SmartHealthCard
{
    public class ShcIssuersDto
    {
        [JsonPropertyName("participating_issuers")]
        public ShcIssuerDto[] ParticipatingIssuers { get; set; }
    }

    public class ShcIssuerDto
    {
        [JsonPropertyName("iss")]
        public string Iss { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("canonical_iss")]
        public string CanonicalIss { get; set; }
        [JsonPropertyName("website")]
        public string Website { get; set; }
    }
}
