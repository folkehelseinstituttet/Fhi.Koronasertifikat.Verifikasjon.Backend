using System;

namespace FHICORC.Application.Models.Revocation
{
    public class FilterRevocDto
    {
        public int BatchId { get; set; }
        public byte[] Filter { get; set; }
        public DateTime Date { get; set; }
    }
}
