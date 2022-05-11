using FHICORC.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHICORC.Application.Models
{
    public class HashDto
    {
        public HashDto(RevocationHash hash)
        {
            this.Id = hash.Id;
            this.BatchId = hash.BatchId;
            this.HashInfo = hash.Hash;
        }
        public int Id { get; set; }
        public string BatchId { get; set; }
        public string HashInfo { get; set; }

    }
}
