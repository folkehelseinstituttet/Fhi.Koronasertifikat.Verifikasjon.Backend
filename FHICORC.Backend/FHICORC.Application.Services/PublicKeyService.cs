﻿using Microsoft.Extensions.Logging;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.Application.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using FHICORC.Application.Common.Interfaces;

namespace FHICORC.Application.Services
{
    public class PublicKeyService : IPublicKeyService
    {
        private const string CacheKey = "publicKeyCacheKey";

        private readonly ICacheManager _cacheManager;
        private readonly PublicKeyCacheOptions _publicKeyCacheOptions;
        private readonly ILogger<PublicKeyService> _logger;
        private readonly IEuCertificateRepository _euCertificateRepository;
        private readonly IMetricLogService _metricLogService;
        private readonly IJsonPublicKeyService _jsonPublicKeyService;

        public PublicKeyService(ILogger<PublicKeyService> logger, ICacheManager cacheManager, PublicKeyCacheOptions publicKeyCacheOptions,
                                IEuCertificateRepository euCertificateRepository, IMetricLogService metricLogService,
                                IJsonPublicKeyService jsonPublicKeyService)

        {
            _cacheManager = cacheManager;
            _publicKeyCacheOptions = publicKeyCacheOptions;
            _euCertificateRepository = euCertificateRepository; 
            _logger = logger;
            _metricLogService = metricLogService;
            _jsonPublicKeyService = jsonPublicKeyService;
        }

        public async Task<PublicKeyResponseDto> GetPublicKeysAsync()
        {
            if (_cacheManager.TryGetValue(CacheKey, out PublicKeyResponseDto cachedData))
            {
                _logger.LogDebug("PublicKeys found in cache");
                _metricLogService.AddMetric("PublicKey_CacheHit", true);
                return cachedData;
            }
            _logger.LogDebug("PublicKeys not found in cache. Fetching from repository");
            _metricLogService.AddMetric("PublicKey_CacheHit", false);

            var databaseResults = await _euCertificateRepository.GetAllEuDocSignerCertificates();

            var publicKeyResponseDto = new PublicKeyResponseDto()
            {
                pkList = databaseResults.Select(x => new CertificatePublicKey
                {
                    kid = x.KeyIdentifier,
                    publicKey = x.PublicKey
                }).ToList()
            };

            publicKeyResponseDto = await SeedAdditionalDscsFromJson(publicKeyResponseDto);
            
            _cacheManager.Set(CacheKey, publicKeyResponseDto, _publicKeyCacheOptions);

            return publicKeyResponseDto;
        }

        private async Task<PublicKeyResponseDto> SeedAdditionalDscsFromJson(PublicKeyResponseDto responseDto)
        {
            var jsonPublicKeys = await _jsonPublicKeyService.GetPublicKeysAsync();
            foreach (var jsonPk in jsonPublicKeys.pkList)
            {
                if (!responseDto.pkList.Exists(pk => pk.kid.Equals(jsonPk.kid)))
                {
                    responseDto.pkList.Add(jsonPk);
                }
            }

            return responseDto;
        }
    }
}
