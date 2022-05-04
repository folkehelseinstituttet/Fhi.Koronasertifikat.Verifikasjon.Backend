using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FHICORC.Domain.Models
{
    public class RevocationHash
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(RevocationBatch))]
        public string BatchId { get; set; }

        public string Hash { get; set; }

        public virtual RevocationBatch RevocationBatch { get; set; }
    }
}
