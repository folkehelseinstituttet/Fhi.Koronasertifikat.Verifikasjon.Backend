using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FHICORC.Domain.Models
{
    public class RevocationSuperFilter
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string SuperCountry { get; set; }

        public DateTime SuperExpires { get; set; }

        [MaxLength(5992)]
        public byte[] SuperFilter { get; set; }

        public int BatchCount { get; set; }

        public DateTime Modified { get; set; }



        public virtual ICollection<RevocationBatch> RevocationBatches { get; set; }
    }
}
