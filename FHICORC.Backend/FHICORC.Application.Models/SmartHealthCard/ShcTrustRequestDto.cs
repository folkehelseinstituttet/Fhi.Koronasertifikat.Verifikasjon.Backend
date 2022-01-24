using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FHICORC.Application.Models.SmartHealthCard
{
    public class ShcTrustRequestDto
    {
        [FromHeader]
        [Required]
        [JsonPropertyName("iss")]
        public string iss { get; set; }
    }
}
