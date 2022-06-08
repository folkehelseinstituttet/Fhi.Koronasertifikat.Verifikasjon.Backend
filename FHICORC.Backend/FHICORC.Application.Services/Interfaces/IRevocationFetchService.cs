using System;
using System.Collections.Generic;
using FHICORC.Application.Models;

namespace FHICORC.Application.Services
{
    public interface IRevocationFetchService
    {
        bool ContainsCertificate(string dcc, string country);
        IEnumerable<SuperBatch> FetchSuperBatches(DateTime dateTime);
        IEnumerable<BucketItem> FetchBucketInfo();
    }
}
