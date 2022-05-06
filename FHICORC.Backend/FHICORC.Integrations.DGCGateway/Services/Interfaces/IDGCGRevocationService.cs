using FHICORC.Application.Models;
using System.Threading.Tasks;

namespace FHICORC.Integrations.DGCGateway.Services
{
    public interface IDGCGRevocationService
    {
        public Task PopulateRevocationDatabase(DgcgRevocationBatchListRespondDto revocationBatchList);
        public void AddToDatabase(DgcgRevocationListBatchItem batchRoot, DGCGRevocationBatchRespondDto batch);
        public void DeleteExpiredBatches();

    }
}
