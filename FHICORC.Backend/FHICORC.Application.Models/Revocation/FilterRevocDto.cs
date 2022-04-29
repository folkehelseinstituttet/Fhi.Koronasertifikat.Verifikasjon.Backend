using System;

namespace FHICORC.Application.Models.Revocation
{
    public class FilterRevocDto
    {
        public string BatchId { get; set; }
        public byte[] Filter { get; set; }
        public DateTime Date { get; set; }
    }
}
