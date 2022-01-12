
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FHICORC.Application.Models
{
    public class ShcTrustRequestDto
    {
        [FromHeader]
        [Required]
        public string iss { get; set; }
    }
}
