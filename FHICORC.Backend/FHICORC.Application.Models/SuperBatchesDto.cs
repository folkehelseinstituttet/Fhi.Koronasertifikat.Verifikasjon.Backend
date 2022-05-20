using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHICORC.Application.Models
{
    public class SuperBatchesDto
    {
        public List<SuperBatch> SuperBatches { get; set; }
    }

    public class SuperBatch{
        public int Id { get; set; }
        public string Country { get; set; }
        public int BucketId { get; set; }
        public byte[] SuperFilter { get; set; }

    }
}
