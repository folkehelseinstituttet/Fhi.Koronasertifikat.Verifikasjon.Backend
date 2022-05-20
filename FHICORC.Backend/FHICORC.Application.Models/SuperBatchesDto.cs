namespace FHICORC.Application.Models
{
    public class SuperBatch{
        public int Id { get; set; }
        public string CountryISO3166 { get; set; }
        public int BucketType { get; set; }
        public byte[] BloomFilter { get; set; }

    }
}
