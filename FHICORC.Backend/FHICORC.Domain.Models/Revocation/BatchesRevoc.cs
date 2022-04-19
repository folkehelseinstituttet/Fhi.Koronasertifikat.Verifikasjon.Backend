using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FHICORC.Domain.Models
{
    public class BatchesRevoc
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BatchId { get; set; }
        public DateTime Expires { get; set; }
        public DateTime Date { get; set; }
        public string Country { get; set; }
        public bool Deleted { get; set; }
        public string Kid { get; set; }
        public string HashType { get; set; }
        public bool Upload { get; set; }
    }
}
