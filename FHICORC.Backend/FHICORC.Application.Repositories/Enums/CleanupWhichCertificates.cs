using System;

namespace FHICORC.Application.Repositories.Enums
{
    [Flags]
    public enum CleanupWhichCertificates
    {
        AllButUkCertificates = 1,
        UkCertificates = 2,
        UkNiCertificates = 4,
        All = 7
    }
}
