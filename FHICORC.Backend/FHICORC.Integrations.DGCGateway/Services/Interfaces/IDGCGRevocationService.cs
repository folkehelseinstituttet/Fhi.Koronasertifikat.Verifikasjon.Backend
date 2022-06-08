using FHICORC.Application.Models;
using System.Threading.Tasks;

namespace FHICORC.Integrations.DGCGateway.Services
{
    public interface IDGCGRevocationService
    {
        Task PopulateRevocationDatabase(DgcgRevocationBatchListRespondDto revocationBatchList);
        void AddToDatabase(DgcgRevocationListBatchItem batchRoot, DGCGRevocationBatchRespondDto batch);
        void DeleteExpiredBatches();

    }
}
