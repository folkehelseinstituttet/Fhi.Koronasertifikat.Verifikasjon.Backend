using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FHICORC.Domain.Models
{
    public class FiltersRevoc
    {
        [Key]
        [ForeignKey(nameof(BatchesRevoc))]
        public int BatchId { get; set; }

        [MaxLength(5992)]
        public byte[] Filter { get; set; }

        public virtual BatchesRevoc BatchesRevoc { get; set; }
    }
}
