using FHICORC.Application.Models;

namespace FHICORC.Integrations.DGCGateway.Services
{
    public interface IDGCGRevocationService
    {
        public void AddToDatabase(DgcgRevocationListBatchItem batchRoot, DGCGRevocationBatchRespondDto batch);
        public void DeleteExpiredBatches();

    }
}
