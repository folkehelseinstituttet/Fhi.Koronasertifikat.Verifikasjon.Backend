using System.Text;
using System.Collections;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using FHICORC.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;
using FHICORC.Integrations.DGCGateway.Util;
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

        public bool ContainsCertificate(string dcc) {
            return ContainsCertificateFilter(dcc);
        }

        public bool ContainsCertificateFilter(string str) {
            var hashData = BloomFilterUtils.HashData(Encoding.UTF8.GetBytes(str), 47936, 32);

            foreach (var bf in _coronapassContext.RevocationSuperFilter)
            {
                var a = new BitArray(bf.SuperFilter);
                var contains = a.Contains(hashData);
                if (contains)
                    return true;
            }
            return false;
        }


        public SuperBatchesDto FetchSuperBatches(DateTime dateTime) {
            var superBatchList = _coronapassContext.RevocationSuperFilter
                .Where(s => s.Modified <= dateTime)
                .Select(x => new SuperBatch()
                {
                    Id = x.Id,
                    SuperFilter = x.SuperFilter,
                }
                ).ToList();

            return new SuperBatchesDto()
            {
                SuperBatches = superBatchList
            };           
        }     
    }
}
