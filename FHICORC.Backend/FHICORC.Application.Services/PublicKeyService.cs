using Microsoft.Extensions.Logging;
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

        public PublicKeyService(ILogger<PublicKeyService> logger, ICacheManager cacheManager, PublicKeyCacheOptions publicKeyCacheOptions, IEuCertificateRepository euCertificateRepository)

        {
            _cacheManager = cacheManager;
            _publicKeyCacheOptions = publicKeyCacheOptions;
            _euCertificateRepository = euCertificateRepository; 
            _logger = logger;
        }

        public async Task<PublicKeyResponseDto> GetPublicKeysAsync()
        {
            if (_cacheManager.TryGetValue(CacheKey, out PublicKeyResponseDto cachedData))
            {
                _logger.LogDebug("PublicKeys found in cache");
                return cachedData;
            }
            _logger.LogDebug("PublicKeys not found in cache. Fetching from repository");

            var databaseResults = await _euCertificateRepository.GetAllEuDocSignerCertificates();

            var publicKeyResponseDto = new PublicKeyResponseDto()
            {
                pkList = databaseResults.Select(x => new CertificatePublicKey
                {
                    kid = x.KeyIdentifier,
                    publicKey = x.PublicKey
                }).ToList()
            };

            _cacheManager.Set(CacheKey, publicKeyResponseDto, _publicKeyCacheOptions);

            return publicKeyResponseDto;
        }
    }
}
