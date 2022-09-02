using System;
using System.Collections.Generic;

namespace FHICORC.Application.Models
{
    public class DGCGRevocationBatchRespondDto
    {
        public string Country { get; set; }
        public DateTime Expires { get; set; }
        public string Kid { get; set; }
        public string HashType { get; set; }
        public List<string> Entries { get; set; }
        
    }

    public class DgcgHashItem
    {
        public string Hash { get; set; }
    }

}