using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FHICORC.Application.Models
{
    public class HashesDto
    {
        public List<Hash> Hashes { get; set; }
    }

    public class Hash
    {
        public int Id { get; set; }
        public string BatchId { get; set; }
        public string HashInfo { get; set; }
    }
}
