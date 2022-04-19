using System.Text;
using System.Collections;
using System.Security.Cryptography;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using FHICORC.Infrastructure.Database.Context;

namespace FHICORC.Application.Services
{
    public interface IBloomFilterService
    {

        public void CustomeFilter();
        public bool ContainsCertificate();
        public void AddToFilterTest(int numberOfHashes = 1000);

    }
}
