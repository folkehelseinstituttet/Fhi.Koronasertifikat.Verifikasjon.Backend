using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace FHICORC.Application.Repositories.Interfaces
{
    public interface IBusinessRuleRepository
    {
        Task<JToken> GetAllBusinessRules();
    }
}