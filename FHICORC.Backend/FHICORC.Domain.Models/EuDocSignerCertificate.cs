using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; 

namespace FHICORC.Domain.Models
{
    public class EuDocSignerCertificate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int EuDocSignerCertificateId { get; set; }
        [Column(TypeName = "varchar(50)")]
        public string KeyIdentifier { get; set; }
        [Column(TypeName = "varchar(10)")]
        public string Country { get; set; }
        [Column(TypeName = "varchar(10)")]
        public string CertificateType { get; set; }
        [Column(TypeName = "varchar(100)")]
        public string Thumbprint { get; set; }
        public string Signature { get; set; }
        public string Certificate { get; set; }
        public string PublicKey { get; set; }
        public DateTime Timestamp { get; set; }
        public DateTime Created { get; set; }

    }
}
