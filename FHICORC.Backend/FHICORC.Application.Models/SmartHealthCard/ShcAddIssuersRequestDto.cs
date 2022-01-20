namespace FHICORC.Application.Models.SmartHealthCard
{

    public class AddIssuersRequest
    {
        public Issuer[] issuers { get; set; }
    }

    public class Issuer
    {
        public string issuer { get; set; }
        public string name { get; set; }
    }

}
