using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FHICORC.Domain.Models
{
    public class BusinessRule
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BusinessRuleId { get; set; }
        [Column(TypeName = "varchar(20)")]
        public string RuleIdentifier { get; set; }
        [Column(TypeName = "varchar(20000)")]
        public string RuleJson { get; set; }
        public DateTime Created { get; set; }
    }
}
