using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FHICORC.Domain.Models
{
    public class FiltersRevoc
    {
        [Key]
        public string BatchId { get; set; }

        [MaxLength(5992)]
        public byte[] Filter { get; set; }

        public virtual RevocationBatch RevocationBatch { get; set; }
    }
}
