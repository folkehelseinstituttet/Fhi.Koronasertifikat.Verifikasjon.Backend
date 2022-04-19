﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FHICORC.Domain.Models
{
    public class HashesRevoc
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(BatchesRevoc))]
        public int BatchId { get; set; }

        public string Hash { get; set; }

        public virtual BatchesRevoc BatchesRevoc { get; set; }
    }
}