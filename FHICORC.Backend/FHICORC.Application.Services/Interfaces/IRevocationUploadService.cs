using System.Collections.Generic;

namespace FHICORC.Application.Services
{
    public interface IRevocationUploadService
    {
        public bool UploadHashes(IEnumerable<string> newHashes);
    }
}
