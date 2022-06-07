using System.Collections.Generic;

namespace FHICORC.Application.Services
{
    public interface IRevocationUploadService
    {
        bool UploadHashes(IEnumerable<string> newHashes);
    }
}
