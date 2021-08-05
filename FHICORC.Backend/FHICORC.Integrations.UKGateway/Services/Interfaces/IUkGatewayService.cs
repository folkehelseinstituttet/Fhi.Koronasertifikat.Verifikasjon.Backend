using System.Collections.Generic;
using System.Threading.Tasks;
using FHICORC.Domain.Models;

namespace FHICORC.Integrations.UkGateway.Services.Interfaces
{
    public interface IUkGatewayService
    {
        Task<List<EuDocSignerCertificate>> GetTrustListAsync();
    }
}
