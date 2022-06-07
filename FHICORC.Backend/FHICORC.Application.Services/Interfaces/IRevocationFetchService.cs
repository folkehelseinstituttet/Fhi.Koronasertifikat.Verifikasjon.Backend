using System;
using System.Collections.Generic;
using FHICORC.Application.Models;

namespace FHICORC.Application.Services
{
    public interface IRevocationFetchService
    {
        public bool ContainsCertificate(string dcc, string country);
        public IEnumerable<SuperBatch> FetchSuperBatches(DateTime dateTime);
        public IEnumerable<BucketItem> FetchBucketInfo();
    }
}
