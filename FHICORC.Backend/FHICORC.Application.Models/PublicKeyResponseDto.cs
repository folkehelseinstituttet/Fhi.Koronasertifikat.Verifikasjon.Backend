using System.Collections.Generic;

namespace FHICORC.Application.Models
{
    public class PublicKeyResponseDto
    {
        public List<CertificatePublicKey> pkList { get; set; }
    }

    public class CertificatePublicKey
    {
        public string kid { get; set; }
        public string publicKey { get; set; }
    }
}
