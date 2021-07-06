using System;
using Microsoft.AspNetCore.Mvc;

namespace FHICORC.Application.Models
{
    public class ValueSetRequestDto
    {
        [FromHeader]
        public DateTimeOffset? LastFetched { get; set; }
    }
}
