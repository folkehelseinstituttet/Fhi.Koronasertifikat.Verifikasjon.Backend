namespace FHICORC.Application.Models.Options
{
    public class CertificateOptions
    {
        /// <summary>
        /// National Backend TLS certificate path (only specify for loading via mounting)
        /// </summary>
        public string NBTlsCertificatePath { get; set; }

        /// <summary>
        /// National Backend TLS certificate password (only specify for loading via mounting)
        /// </summary>
        public string NBTlsCertificatePassword { get; set; }

        /// <summary>
        /// Key Vault Url for retrieving National Backend TLS certificate (only specify for loading from Key Vault)
        /// </summary>
        public string NBTlsKeyVaultUrl { get; set; }

        /// <summary>
        /// National Backend TLS certificate name in Key Vault (only specify for loading from Key Vault)
        /// </summary>
        public string NBTlsKeyVaultCertificateName { get; set; }

        /// <summary>
        /// Path to trust anchor public key
        /// </summary>
        public string DGCGTrustAnchorPath { get; set; }

        /// <summary>
        /// Set to true to disable validation of DGCG TLS certificate
        /// </summary>
        public bool DisableDGCGServerCertValidation { get; set; }
    }
}
