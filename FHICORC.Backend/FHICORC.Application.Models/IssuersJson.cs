using Newtonsoft.Json;
using System.Collections.Generic;

namespace FHICORC.Application.Models
{
    public class Rootobject
    {
        public Participating_Issuers[] participating_issuers { get; set; }
    }

    public class Participating_Issuers
    {
        public string iss { get; set; }
        public string name { get; set; }
        public string canonical_iss { get; set; }
        public string website { get; set; }
    }
}