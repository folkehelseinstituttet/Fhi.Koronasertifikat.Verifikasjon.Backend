using System.Text.Json.Serialization;

namespace FHICORC.Application.Models.SmartHealthCard
{
    public class VaccineCodesDto
    {
        [JsonPropertyName("codes")]
        public CodeDto[] Codes { get; set; }
    }

    public class CodeDto
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("system")]
        public string System { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("manufacturer")]
        public string Manufacturer { get; set; }
        [JsonPropertyName("type")]
        public string Type { get; set; }
        [JsonPropertyName("target")]
        public string Target { get; set; }
    }


}
