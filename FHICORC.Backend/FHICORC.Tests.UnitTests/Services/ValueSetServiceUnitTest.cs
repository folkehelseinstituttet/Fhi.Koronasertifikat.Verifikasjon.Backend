using System;
using System.Threading.Tasks;
using FHICORC.Application.Common;
using FHICORC.Application.Common.Interfaces;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace FHICORC.Tests.UnitTests.Services
{
    [Category("Unit")]
    public class ValueSetServiceUnitTest
    {
        private readonly Mock<ValueSetCacheOptions> _mockValueSetCacheOptions = new Mock<ValueSetCacheOptions>();
        private IServiceCollection _services = new ServiceCollection();
        private IServiceProvider _serviceProvider;
        private ICacheManager _cacheManager;
        private const string ValueSetCacheKey = "VALUESETS_CACHE_KEY";
        private readonly ValueSetOptions _valueSetOptions = new ValueSetOptions() { ValueSetsDirectory = "../../../../../AppValueSets" };
        private readonly Mock<ILogger<ValueSetService>> _loggerMock = new Mock<ILogger<ValueSetService>>();
        private readonly Mock<IMetricLogService> _mockMetricLogService = new Mock<IMetricLogService>();
        private readonly ZipManager _zipManager = new ZipManager();
        private ValueSetService _valueSetService;
        private readonly byte[] _cacheData = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };

        [SetUp]
        public void Setup()
        {
            //Cache setup
            _mockValueSetCacheOptions.Object.AbsoluteExpiration = 1440;
            _mockValueSetCacheOptions.Object.SlidingExpiration = 1440;
            _mockValueSetCacheOptions.Object.CacheSize = 1024;
            _services = new ServiceCollection();
            _services.AddMemoryCache();
            _services.AddSingleton<ICacheManager, CacheManager>();
            _serviceProvider = _services.BuildServiceProvider();
            _cacheManager = _serviceProvider.GetService<ICacheManager>();

            //ValueSet Service instance 
            _valueSetService = new ValueSetService(_loggerMock.Object, _cacheManager, _mockValueSetCacheOptions.Object, _valueSetOptions, _mockMetricLogService.Object, _zipManager);
        }

        [Test]
        public async Task GetLatestVersion_ShouldReturnZippedByteArray_AndRetrieveFromCache_WhenServerIsNewerThanApp()
        {
            // Arrange //
            ValueSetResponseDto cachedData;

            ValueSetRequestDto vsRequest = new ValueSetRequestDto();

            // Act //
            var vsResponse = await _valueSetService.GetLatestVersionAsync(vsRequest);
            _cacheManager.TryGetValue(ValueSetCacheKey, out cachedData);

            // Assert //
            Assert.IsFalse(vsResponse.IsAppVersionUpToDate);
            Assert.IsTrue(vsResponse.ZipContents != null && vsResponse.ZipContents.Length > 0);  // Check that zipContents is actually a byte array and that it has bytes
            Assert.IsTrue(vsResponse.IsZipFileCreated);                                              // Check if zipfiles are created
            Assert.AreEqual(cachedData.ZipContents, vsResponse.ZipContents);                                     // Check that we can retrieve the zipped contents from the cache and that they're equal
        }

        [Test]
        public async Task GetLatestContents_ShouldReturnEmptyContent_WhenAppIsNewerThanServerVersion()
        {
            // Arrange //
            ValueSetResponseDto cachedData;

            ValueSetRequestDto vsRequest = new ValueSetRequestDto { LastFetched = DateTime.MaxValue };

            // Act //
            var vsResponse = await _valueSetService.GetLatestVersionAsync(vsRequest);
            var cacheResponse = _cacheManager.TryGetValue(ValueSetCacheKey, out cachedData);

            // Assert //
            Assert.IsTrue(vsResponse.IsAppVersionUpToDate);
            Assert.IsTrue(vsResponse.ZipContents is null);
            Assert.IsFalse(vsResponse.IsZipFileCreated);
            Assert.IsTrue(vsResponse.IsAppVersionUpToDate);
            Assert.IsTrue(cacheResponse);
        }

        [Test]
        public async Task File_Cache_Is_Created_When_Empty()
        {
            // Arrange //
            ValueSetResponseDto cachedData;
            ValueSetRequestDto vsRequest = new ValueSetRequestDto();
            _cacheManager.TryGetValue(ValueSetCacheKey, out cachedData);

            Assert.True(cachedData == null);

            // Act //
            var valueSetResponse = await _valueSetService.GetLatestVersionAsync(vsRequest);
            var cacheResponse = _cacheManager.TryGetValue(ValueSetCacheKey, out cachedData);

            // Assert //
            Assert.IsTrue(cacheResponse);
            Assert.IsTrue(cachedData != null);
        }

        [Test]
        public async Task Cache_Contains_Expected_Data()
        {
            // Arrange //
            ValueSetResponseDto cachedData;
            _cacheManager.Set(ValueSetCacheKey, _cacheData, _mockValueSetCacheOptions.Object);
            ValueSetService specificValueSetService = new ValueSetService(_loggerMock.Object, _cacheManager, _mockValueSetCacheOptions.Object, _valueSetOptions, _mockMetricLogService.Object, _zipManager);

            // Act //
            ValueSetRequestDto vsRequest = new ValueSetRequestDto();
            var valueSetResponse = await _valueSetService.GetLatestVersionAsync(vsRequest);

            _cacheManager.TryGetValue(ValueSetCacheKey, out cachedData);

            Assert.IsTrue(cachedData != null);
            Assert.AreEqual(valueSetResponse.ZipContents, cachedData.ZipContents);
        }
    }
}
