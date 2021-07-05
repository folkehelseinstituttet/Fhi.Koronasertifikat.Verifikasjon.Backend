using System;
using Microsoft.AspNetCore.Mvc;

namespace FHICORC.Application.Models
{
    public class ValueSetRequestDto
    {
        [FromHeader]
        public DateTime? LastFetched { get; set; }
    }
}
