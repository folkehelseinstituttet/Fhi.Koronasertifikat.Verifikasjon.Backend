using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FHICORC.Domain.Models
{
    public class RevocationBatch
    {
        [Key]
        public string BatchId { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Date { get; set; }
        public string Country { get; set; }
        public bool Deleted { get; set; }
        public string Kid { get; set; }
        public string HashType { get; set; }
        public bool Upload { get; set; }

        [ForeignKey(nameof(RevocationSuperFilter))]
        public int? SuperId { get; set; }

     
        public virtual RevocationSuperFilter RevocationSuperFilter { get; set; }
        public virtual RevocationFilter RevocationFilter { get; set; }

        public virtual ICollection<RevocationHash> RevocationHashes { get; set; }


    }
}
