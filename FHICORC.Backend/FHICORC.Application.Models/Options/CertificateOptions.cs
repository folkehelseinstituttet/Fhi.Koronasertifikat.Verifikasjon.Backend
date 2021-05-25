using System.ComponentModel.DataAnnotations;

namespace FHICORC.Application.Models.Options
{
    public class CertificateOptions
    {
        public string NBTlsCertificatePath { get; set; }
        public string DGCGTrustAnchorPath { get; set; }
    }
}
