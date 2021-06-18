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
    public class JsonPublicKeyService : IJsonPublicKeyService
    {
        private const string CacheKey = "jsonPublicKeyCacheKey";

        private readonly ICacheManager _cacheManager;
        private readonly PublicKeyCacheOptions _publicKeyCacheOptions;
        private readonly ILogger<PublicKeyService> _logger;
        private readonly ICertificatePublicKeyRepository _certificatePublicKeyRepository;

        public JsonPublicKeyService(ILogger<PublicKeyService> logger, ICacheManager cacheManager, PublicKeyCacheOptions publicKeyCacheOptions, ICertificatePublicKeyRepository certificatePublicKeyRepository)
        {
            _cacheManager = cacheManager;
            _publicKeyCacheOptions = publicKeyCacheOptions;
            _certificatePublicKeyRepository = certificatePublicKeyRepository;
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

            var repoDictionary = await _certificatePublicKeyRepository.GetPublicKeysFromFileAsync();

            var publicKeyResponseDto = new PublicKeyResponseDto()
            {
                pkList = repoDictionary.Select(kvp => new CertificatePublicKey
                {
                    kid = kvp.Key,
                    publicKey = kvp.Value
                }).ToList()
            };

            _cacheManager.Set(CacheKey, publicKeyResponseDto, _publicKeyCacheOptions);

            return publicKeyResponseDto;
        }
    }
}