using System.Threading.Tasks;
using FHICORC.Application.Models;

namespace FHICORC.Application.Services.Interfaces
{
    public interface IValueSetService
    {
        Task<ValueSetResponseDto> GetLatestVersionAsync(ValueSetRequestDto valueSetRequestDto);
    }
}
