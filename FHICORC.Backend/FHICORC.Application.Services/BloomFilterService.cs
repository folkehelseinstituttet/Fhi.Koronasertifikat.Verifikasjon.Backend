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

namespace FHICORC.Application.Services
{
    public class BloomFilterService : IBloomFilterService
    {
        private readonly ILogger<BloomFilterService> _logger;
        private readonly CoronapassContext _coronapassContext;

        public BloomFilterService(ILogger<BloomFilterService> logger, CoronapassContext coronapassContext)
        {
            _logger = logger;
            _coronapassContext = coronapassContext;
        }

        public bool ContainsCertificate(string dcc) {
            return ContainsCertificateFilter(dcc);
        }

        public bool ContainsCertificateFilter(string str) {
            var hashData = BloomFilterUtils.HashData(Encoding.UTF8.GetBytes(str), 47936, 32);
            
            foreach (var bf in _coronapassContext.SuperFiltersRevoc)
            {
                var a = new BitArray(bf.SuperFilter);
                var contains = a.Contains(hashData);
                if (contains)
                    return true;
            }
            return false;
        }
    }
}
