using System;
using System.Collections.Generic;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Revocation;

namespace FHICORC.Application.Services
{
    public interface IRevocationFetchService
    {
        IEnumerable<SuperBatch> FetchSuperBatches(DateTime dateTime);
        IEnumerable<int> FetchSuperBatchRevocationList(DateTime dateTime);
        SuperBatch FetchSuperBatch(int id);
        SuperBatchChunkDto FetchSuperBatchesChunk(DateTime dateTime);
    }
}
