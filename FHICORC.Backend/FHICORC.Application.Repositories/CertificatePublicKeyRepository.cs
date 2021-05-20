using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FHICORC.Application.Repositories
{
    public class CertificatePublicKeyRepository : ICertificatePublicKeyRepository
    {
        private readonly SecurityOptions _securityOptions;
        private readonly ILogger<CertificatePublicKeyRepository> _logger;
        public CertificatePublicKeyRepository(SecurityOptions securityOptions, ILogger<CertificatePublicKeyRepository> logger)
        {
            _securityOptions = securityOptions;
            _logger = logger;
        }

        public async Task<Dictionary<string, string>> GetPublicKeysFromFileAsync()
        {
            var resultDict = new Dictionary<string, string>();

            string[] file = Directory.GetFiles(_securityOptions.PublicKeyCertificatePath);
            if (file.Length != 1)
            {
                _logger.LogError("Zero or more than one publicKey file found in {PublicKeyCertificatePath}", _securityOptions.PublicKeyCertificatePath);
                throw new InvalidOperationException("Zero or more than one publicKey file found.");
            }

            using (StreamReader r = new StreamReader(file[0]))
            {
                string json = await r.ReadToEndAsync();
                PublicKeyJson datalist = JsonConvert.DeserializeObject<PublicKeyJson>(json);
                
                foreach (Kvp kvp in datalist.kvpList)
                {
                    resultDict.Add(kvp.kid, kvp.pk);
                    _logger.LogDebug("PublicKey for {kid} found.", kvp.kid);
                }
            }
            return resultDict;
        }
    }
}
