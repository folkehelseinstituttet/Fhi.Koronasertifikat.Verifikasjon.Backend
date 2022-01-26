using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FHICORC.Domain.Models
{
    public class TrustedIssuerModel
    {
        [Key]
        [Column(TypeName = "varchar(5000)")]
        public string Iss { get; set; }

        [Column(TypeName = "varchar(5000)")]
        public string Name { get; set; }

        [Column(TypeName = "boolean")]
        public bool IsAddManually { get; set; }
        
        [Column(TypeName = "boolean")]
        public bool IsMarkedUntrusted { get; set; }
    }
}
