using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
