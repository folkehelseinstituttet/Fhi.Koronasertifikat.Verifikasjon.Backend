using System.Text;
using System.Collections;
using System.Security.Cryptography;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using FHICORC.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using FHICORC.Application.Models.Revocation;
using FHICORC.Domain.Models;
using FHICORC.Application.Models;

namespace FHICORC.Application.Services
{
    public class RevocationService : IRevocationService
    {
        private readonly ILogger<RevocationService> _logger;
        private readonly CoronapassContext _coronapassContext;

        public RevocationService(ILogger<RevocationService> logger, CoronapassContext coronapassContext)
        {
            _logger = logger;
            _coronapassContext = coronapassContext;
        }

        public void AddToDatabase(DgcgRevocationListBatchItem batchRoot, DGCGRevocationBatchRespondDto batch) {

            var batchesRevoc = new BatchesRevoc()
            {
                BatchId = 43434,
                Expires = batch.Expires,
                Date = batchRoot.Date,
                Country = batchRoot.Country,
                Deleted = batchRoot.Deleted,
                Kid = batch.Kid,
                HashType = batch.HashType,
                Upload = false,
            };

            _coronapassContext.BatchesRevoc.Add(batchesRevoc);
            _coronapassContext.SaveChanges();


        }



    }
}
