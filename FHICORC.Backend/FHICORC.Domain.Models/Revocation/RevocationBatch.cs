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
        public int HashType { get; set; }
        public bool Upload { get; set; }

        [ForeignKey(nameof(RevocationSuperFilter))]
        public int? SuperId { get; set; }

     
        public virtual RevocationSuperFilter RevocationSuperFilter { get; set; }

        public virtual ICollection<RevocationHash> RevocationHashes { get; set; }

        public RevocationBatch(string batchId, DateTime expires, string country, bool deleted, string hashType, bool upload)
        {
            this.BatchId = batchId;
            this.Expires = expires;
            this.Country = country;
            this.Deleted = deleted;
            this.HashType = hashType;
            this.Upload = upload;
        }

        public RevocationBatch(string batchId, DateTime expires, DateTime date, string country, bool deleted, string kid, string hashType, bool upload)
        {
            this.BatchId = batchId;
            this.Expires = expires;
            this.Date = date;
            this.Country = country;
            this.Deleted = deleted;
            this.Kid = kid;
            this.HashType = hashType;
            this.Upload = upload;
        }

    }
}
