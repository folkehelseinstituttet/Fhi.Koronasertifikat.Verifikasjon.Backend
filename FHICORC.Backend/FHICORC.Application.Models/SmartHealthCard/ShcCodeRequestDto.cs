using System.Text.Json.Serialization;

namespace FHICORC.Application.Models.SmartHealthCard
{
    public class ShcCodeRequestDto
    {
        [JsonPropertyName("codes")]
        public ShcCodeEntries[] Codes { get; set; }
    }

    public class ShcCodeEntries
    {
        [JsonPropertyName("code")]
        public string Code { get; set; }
        [JsonPropertyName("system")]
        public string System { get; set; }
    }

    public static class CodingSystem
    {
        public const string Cvx = "http://hl7.org/fhir/sid/cvx";
        public const string Atc = "http://www.whocc.no/atc";
        public const string Gtin = "https://www.gs1.org/gtin";
        public const string Scomed = "http://snomed.info/sct";
        public const string Mms = "http://id.who.int/icd/release/11/mms";
    }
}
