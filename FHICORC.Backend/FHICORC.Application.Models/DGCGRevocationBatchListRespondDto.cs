using System;
using System.Collections.Generic;

namespace FHICORC.Application.Models
{
    public class DgcgRevocationBatchListRespondDto
    {
        public bool More { get; set; }
        public List<DgcgRevocationListBatchItem> Batches { get; set; }

        public DgcgRevocationBatchListRespondDto(bool more, List<DgcgRevocationListBatchItem> batches) { 
            More = more;
            Batches = batches;
        }
    }

    public class DgcgRevocationListBatchItem
    {
        public string BatchId { get; set; }
        public string Country { get; set; }
        public DateTime Date { get; set; }
        public bool Deleted { get; set; }
    }
    
}
