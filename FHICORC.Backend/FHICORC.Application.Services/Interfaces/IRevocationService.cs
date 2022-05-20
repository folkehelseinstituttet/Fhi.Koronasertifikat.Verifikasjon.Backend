using System.Text;
using System.Collections;
using System.Security.Cryptography;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using FHICORC.Infrastructure.Database.Context;
using FHICORC.Application.Models.Revocation;
using System.Collections.Generic;
using FHICORC.Application.Models;

namespace FHICORC.Application.Services
{
    public interface IRevocationService
    {
        public bool ContainsCertificate(string dcc, string country);
        public List<SuperBatch> FetchSuperBatches(DateTime dateTime);
        public BloomFilterBuckets FetchBucketInfo();
    }
}
