using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FHICORC.Domain.Models
{
    public class VaccineCodesModel
    {
        [Column(TypeName = "varchar(200)")]
        public string VaccineCode { get; set; }

        [Key]
        [Column(TypeName = "varchar(2000)")]
        public string CodingSystem { get; set; }
        
        [Column(TypeName = "varchar(500)")]
        public string Name { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string Manufacturer { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string Type { get; set; }

        [Column(TypeName = "varchar(1000)")]
        public string Target { get; set; }
        
        public bool IsAddManually { get; set; }

    }
}
