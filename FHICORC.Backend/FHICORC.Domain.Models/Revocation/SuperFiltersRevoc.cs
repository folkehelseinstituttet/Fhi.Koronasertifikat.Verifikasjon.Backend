using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FHICORC.Domain.Models
{
    public class SuperFiltersRevoc
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime SuperExpires { get; set; }

        [MaxLength(5992)]
        public byte[] SuperFilter { get; set; }

        public bool Changed { get; set; }        

    }
}
