
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FHICORC.Application.Models
{
    public class ShcTrustRequestDto
    {
        [FromHeader]
        [Required]
        [JsonPropertyName("iss")]
        public string Iss { get; set; }
    }
}
