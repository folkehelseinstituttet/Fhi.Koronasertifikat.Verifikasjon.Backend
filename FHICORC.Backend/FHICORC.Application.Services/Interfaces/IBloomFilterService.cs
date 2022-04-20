using System.Text;
using System.Collections;
using System.Security.Cryptography;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using FHICORC.Infrastructure.Database.Context;
using FHICORC.Application.Models.Revocation;
using System.Collections.Generic;

namespace FHICORC.Application.Services
{
    public interface IBloomFilterService
    {

        public bool ContainsCertificate(string dcc);
        public List<FilterRevocDto> GetFilterRevocList();
        public void AddToRevocation(string dcc);

    }
}
