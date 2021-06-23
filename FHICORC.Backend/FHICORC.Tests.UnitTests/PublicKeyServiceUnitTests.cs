using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Repositories.Interfaces;
using FHICORC.Application.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using FHICORC.Application.Common;
using FHICORC.Application.Common.Interfaces;
using FHICORC.Application.Services.Interfaces;
using FHICORC.Domain.Models;

namespace FHICORC.Tests.UnitTests
{
    [Category("Unit")]
    public class PublicKeyServiceUnitTests
    {
        private readonly Mock<PublicKeyCacheOptions> mockPublicKeyCacheOptions = new Mock<PublicKeyCacheOptions>();
        ILogger<PublicKeyService> nullLogger = new NullLoggerFactory().CreateLogger<PublicKeyService>();
        private readonly Mock<IMetricLogService> mockMetricLogService = new Mock<IMetricLogService>();
        private readonly Mock<IEuCertificateRepository> _euCertificateRepositoryMock = new Mock<IEuCertificateRepository>();
        private readonly Mock<IJsonPublicKeyService> _jsonPublicKeyServiceMock = new Mock<IJsonPublicKeyService>();
        private ICacheManager cacheManager;

        List<EuDocSignerCertificate> euDocSignerCertificates = new List<EuDocSignerCertificate>
            {
                new EuDocSignerCertificate
                {
                    KeyIdentifier = "kid",
                    PublicKey = "pk"
                },
                new EuDocSignerCertificate
                {
                    KeyIdentifier = "kid1",
                    PublicKey = "pk2"
                }
            };

        [SetUp]
        public void Setup()
        {
            mockPublicKeyCacheOptions.Object.AbsoluteExpiration = 1440;
            mockPublicKeyCacheOptions.Object.SlidingExpiration = 1440;
            mockPublicKeyCacheOptions.Object.CacheSize = 1024;

            var services = new ServiceCollection();
            services.AddMemoryCache();
            services.AddSingleton<ICacheManager, CacheManager>();
            var serviceProvider = services.BuildServiceProvider();
            cacheManager = serviceProvider.GetService<ICacheManager>();

            _jsonPublicKeyServiceMock.Setup(s => s.GetPublicKeysAsync())
                .Returns(Task.FromResult(new PublicKeyResponseDto {pkList = new List<CertificatePublicKey>()}));
        }

        [Test]
        public void When_PublicKeysAreNotFoundInDB_ShouldThrowError()
        {
            _euCertificateRepositoryMock.Reset();
            PublicKeyService _publicKeyService = new PublicKeyService(nullLogger, cacheManager, mockPublicKeyCacheOptions.Object, _euCertificateRepositoryMock.Object, mockMetricLogService.Object, _jsonPublicKeyServiceMock.Object);

            // Act
            Assert.IsFalse(cacheManager.TryGetValue("publicKeyCacheKey", out PublicKeyResponseDto emptyCachedData));
            var result = _publicKeyService.GetPublicKeysAsync();

            // Assert
            _euCertificateRepositoryMock.Verify(x => x.GetAllEuDocSignerCertificates(), Times.Once());
            Assert.IsNotNull(result.Exception);
        }

        [Test]
        public void When_CacheIsPopulated_FetchDataFromCache()
        {
            // Arrange 
            var publicKeyReponseDto = new PublicKeyResponseDto
            {
                pkList = new List<CertificatePublicKey>
                {
                    new CertificatePublicKey
                    {
                        kid = "kid",
                        publicKey = "pk"
                    }
                }
            };
            
            cacheManager.Set("publicKeyCacheKey", publicKeyReponseDto, mockPublicKeyCacheOptions.Object);
            PublicKeyService _publicKeyService = new PublicKeyService(nullLogger, cacheManager, mockPublicKeyCacheOptions.Object, _euCertificateRepositoryMock.Object, mockMetricLogService.Object, _jsonPublicKeyServiceMock.Object);


            // Act 
            var result = _publicKeyService.GetPublicKeysAsync();

            // Assert
            Assert.AreEqual(result.Result, publicKeyReponseDto);
        }

        [Test]
        public async Task When_CacheIsEmpty_PopulateCache()
        {
            // Arrange 
            _euCertificateRepositoryMock.Setup(p => p.GetAllEuDocSignerCertificates()).ReturnsAsync(euDocSignerCertificates); 
            PublicKeyService _publicKeyService = new PublicKeyService(nullLogger, cacheManager, mockPublicKeyCacheOptions.Object, _euCertificateRepositoryMock.Object, mockMetricLogService.Object, _jsonPublicKeyServiceMock.Object);


            // Act & Assert
            Assert.IsFalse(cacheManager.TryGetValue("publicKeyCacheKey", out PublicKeyResponseDto emptyCachedData));

            await _publicKeyService.GetPublicKeysAsync();

            Assert.IsTrue(cacheManager.TryGetValue("publicKeyCacheKey", out PublicKeyResponseDto cachedData));
        }

        [Test]
        public async Task ResponseIsMappedTo_PublicKeyResponseDto()
        {
            // Arrange 
            _euCertificateRepositoryMock.Setup(p => p.GetAllEuDocSignerCertificates()).ReturnsAsync(euDocSignerCertificates);
            PublicKeyService _publicKeyService = new PublicKeyService(nullLogger, cacheManager, mockPublicKeyCacheOptions.Object, _euCertificateRepositoryMock.Object, mockMetricLogService.Object, _jsonPublicKeyServiceMock.Object);


            // Act & Assert
            var response = await _publicKeyService.GetPublicKeysAsync();
            var type = typeof(PublicKeyResponseDto);
            Assert.True(response.GetType().Equals(type));
            Assert.True(response.pkList[0].kid.Equals("kid"));
            Assert.True(response.pkList[0].publicKey.Equals("pk"));
        }

        [Test]
        public async Task ResultContains_JsonPublicKeys()
        {
            // Arrange 
            _euCertificateRepositoryMock.Setup(p => p.GetAllEuDocSignerCertificates()).ReturnsAsync(euDocSignerCertificates);
            var jsonPublicKeyServiceMock = new Mock<IJsonPublicKeyService>();
            jsonPublicKeyServiceMock.Setup(s => s.GetPublicKeysAsync())
                .Returns(Task.FromResult(new PublicKeyResponseDto {pkList = new List<CertificatePublicKey>
                {
                    new CertificatePublicKey {kid = "jkid", publicKey = "jpk"}
                }}));
            PublicKeyService _publicKeyService = new PublicKeyService(nullLogger, cacheManager, mockPublicKeyCacheOptions.Object, _euCertificateRepositoryMock.Object, mockMetricLogService.Object, jsonPublicKeyServiceMock.Object);

            // Act
            var response = await _publicKeyService.GetPublicKeysAsync();

            // Assert
            var type = typeof(PublicKeyResponseDto);
            Assert.True(response.GetType().Equals(type));
            Assert.True(response.pkList[0].kid.Equals("kid"));
            Assert.True(response.pkList[0].publicKey.Equals("pk"));
            Assert.True(response.pkList[1].kid.Equals("kid1"));
            Assert.True(response.pkList[1].publicKey.Equals("pk2"));
            Assert.True(response.pkList[2].kid.Equals("jkid"));
            Assert.True(response.pkList[2].publicKey.Equals("jpk"));

        }
    }
}