using System.Collections.Generic;
using System.Threading.Tasks;

namespace FHICORC.Application.Repositories.Interfaces
{
    public interface ICertificatePublicKeyRepository
    {
        public Task<Dictionary<string, string>> GetPublicKeysFromFileAsync();
    }
}
