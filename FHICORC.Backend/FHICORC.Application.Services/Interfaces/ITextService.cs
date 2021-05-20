using FHICORC.Application.Models;
using System.Threading.Tasks;

namespace FHICORC.Application.Services.Interfaces
{
    public interface ITextService
    {
        public Task<TextResponseDto> GetLatestVersionAsync(TextRequestDto textRequestDto);
    }
}
