
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FHICORC.Application.Models
{
    public class TextRequestDto
    {
        [FromHeader]
        [Required]
        public string CurrentVersionNo { get; set; }
    }
}
