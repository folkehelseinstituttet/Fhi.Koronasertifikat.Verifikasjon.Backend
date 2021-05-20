using FHICORC.Application.Models;
using System.Threading.Tasks;

namespace FHICORC.Application.Services.Interfaces
{
    public interface IPublicKeyService
    {
        public Task<PublicKeyResponseDto> GetPublicKeysAsync();
    }
}
