using System.Collections.Generic;
using System.Threading.Tasks;
using FHICORC.Domain.Models;

namespace FHICORC.Application.Repositories.Interfaces
{
    public interface IEuCertificateRepository
    {
        Task<bool> PersistEuDocSignerCertificate(EuDocSignerCertificate euDocSignerCertificate);

        Task<bool> CleanupAndPersistEuDocSignerCertificates(List<EuDocSignerCertificate> euDocSignerCertificates);

        Task<List<EuDocSignerCertificate>> GetAllEuDocSignerCertificates();

    }
}
