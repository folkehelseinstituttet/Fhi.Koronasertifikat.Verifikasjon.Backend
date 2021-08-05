namespace FHICORC.Application.Models.Options
{
    public class FeatureToggles
    {
        public bool DisableTrustListVerification { get; set; }
        public bool UseEuDgcGateway { get; set; }
        public bool UseUkGateway { get; set; }
        public bool UseBouncyCastleEuDgcValidation { get; set; }
    }
}
