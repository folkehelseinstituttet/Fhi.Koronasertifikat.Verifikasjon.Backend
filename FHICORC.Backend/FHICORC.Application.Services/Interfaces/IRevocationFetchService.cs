using System;
using System.Collections.Generic;
using FHICORC.Application.Models;

namespace FHICORC.Application.Services
{
    public interface IRevocationFetchService
    {
        IEnumerable<SuperBatch> FetchSuperBatches(DateTime dateTime);
        IEnumerable<int> FetchSuperBatchRevocationList(DateTime dateTime);
        SuperBatch FetchSuperBatch(int id);
    }
}
