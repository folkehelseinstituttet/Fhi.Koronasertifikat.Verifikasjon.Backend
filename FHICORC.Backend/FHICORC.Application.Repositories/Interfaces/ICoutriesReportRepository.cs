using System.Collections.Generic;
using System.Threading.Tasks;

namespace FHICORC.Application.Repositories.Interfaces
{
    public interface ICountriesReportRepository
    {
        Task<IList<string>> GetAllCountries();
    }
}