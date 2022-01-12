
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FHICORC.Application.Models
{
    public class ShcRequestDto
    {
        public Entries[] Codes { get; set; }
    }

    public class Entries
    {
        public string Code { get; set; }
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
