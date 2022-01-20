
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FHICORC.Application.Models
{

    public class AddIssuersRequest
    {
        public Issuer[] issuers { get; set; }
    }

    public class Issuer
    {
        public string issuer { get; set; }
        public string name { get; set; }
    }

}
