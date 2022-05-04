using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHICORC.Application.Models
{
    public class BatchesDto
    {
        public List<Batch> Batches { get; set; }
    }

    public class Batch
    {
        public string BatchId { get; set; }
        public DateTime Expires { get; set; }
        public int SuperId { get; set; }
        public int Count { get; set; }
    }
}
