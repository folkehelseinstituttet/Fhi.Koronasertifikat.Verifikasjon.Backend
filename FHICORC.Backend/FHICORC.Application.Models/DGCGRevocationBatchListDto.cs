using System;
using System.Collections.Generic;

namespace FHICORC.Application.Models
{
    public class DgcgRevocationBatchListDto
    {
        public bool More { get; set; }
        public List<DgcgRevocationBatchListItem> Batches { get; set; }
    }

    public class DgcgRevocationBatchListItem
    {
        public string BatchId { get; set; }
        public string Country { get; set; }
        public DateTime Date { get; set; }
        public bool Deleted { get; set; }
    }
    
}
