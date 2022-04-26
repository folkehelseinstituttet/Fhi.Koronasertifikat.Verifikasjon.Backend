using System;
using System.Collections.Generic;

namespace FHICORC.Application.Models
{
    public class DGCGRevocationBatchRespondDto
    {
        public string Country { get; set; }
        public DateTime expires { get; set; }
        public string Kid { get; set; }
        public string HashType { get; set; }
        public List<DgcgHashItem> Entries { get; set; }
        
    }

    public class DgcgHashItem
    {
        public string Hash { get; set; }
    }

}