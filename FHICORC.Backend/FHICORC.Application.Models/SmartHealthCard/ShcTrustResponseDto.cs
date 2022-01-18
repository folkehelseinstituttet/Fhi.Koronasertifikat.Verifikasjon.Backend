using System.Text.Json.Serialization;

namespace FHICORC.Application.Models
{
    public class ShcTrustResponseDto
    {
        [JsonPropertyName("trusted")]
        public bool Trusted { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
