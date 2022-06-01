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
    public interface IRevocationUploadService
    {
        public bool UploadHashes(IEnumerable<string> newHashes);
    }
}
