using System.Threading.Tasks;
using FHICORC.Application.Models;

namespace FHICORC.Application.Services.Interfaces
{
    public interface IRuleService
    {
        public Task<RuleResponseDto> GetRulesAsync();
    }
}