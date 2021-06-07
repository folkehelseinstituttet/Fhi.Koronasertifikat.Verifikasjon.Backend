using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Services;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FHICORC.Application.Common;
using FHICORC.Application.Common.Interfaces;

namespace FHICORC.Tests.UnitTests
{
    [Category("Unit")]
    public class TextServiceUnitTest
    {
        private readonly DirectoryInfo dirInfo = new DirectoryInfo("../../../../../AppDictionary");
        private readonly Mock<TextCacheOptions> mockTextCacheOptions = new Mock<TextCacheOptions>();
        private IServiceCollection services = new ServiceCollection();
        private IServiceProvider serviceProvider;
        private ICacheManager cacheManager;
        private readonly TextOptions textOptions = new TextOptions() { TextsDirectory = "../../../../../AppDictionary" };
        private readonly Mock<ILogger<TextService>> loggerMock = new Mock<ILogger<TextService>>();
        private readonly Mock<IMetricLogService> mockMetricLogService = new Mock<IMetricLogService>();
        private TextService textService;
        private byte[] cacheData = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
        private string currentFileVersion = "";

        [SetUp]
        public void Setup()
        {
            //Cache setup
            mockTextCacheOptions.Object.AbsoluteExpiration = 1440;
            mockTextCacheOptions.Object.SlidingExpiration = 1440;
            mockTextCacheOptions.Object.CacheSize = 1024;
            services = new ServiceCollection();
            services.AddMemoryCache();
            services.AddSingleton<ICacheManager, CacheManager>();
            serviceProvider = services.BuildServiceProvider();
            cacheManager = serviceProvider.GetService<ICacheManager>();
            //Text Service instance 
            textService = new TextService(loggerMock.Object, cacheManager, mockTextCacheOptions.Object, textOptions, mockMetricLogService.Object);

            //Current file version 
            currentFileVersion = getServerVersion(dirInfo);
        }

        [Test]
        public async Task GetLatestVersion_ShouldReturnZippedByteArray_AndRetrieveFromCache_WhenServerIsNewerThanApp()
        {
            // Arrange //
            byte[] cachedData;

            TextRequestDto _txtRequest = new TextRequestDto() { CurrentVersionNo = "0.9.0" };

            // Act //
            var _txtResponse = await textService.GetLatestVersionAsync(_txtRequest);
            cacheManager.TryGetValue(currentFileVersion, out cachedData);

            // Assert //
            Assert.IsFalse(_txtResponse.IsAppVersionUpToDate);
            Assert.IsTrue(_txtResponse.ZipContents is byte[] && _txtResponse.ZipContents.Length > 0);  // Check that zipContents is actually a byte array and that it has bytes
            Assert.IsTrue(_txtResponse.IsZipFileCreated);                                              // Check if zipfiles are created
            Assert.AreEqual(cachedData, _txtResponse.ZipContents);                                     // Check that we can retrieve the zipped contents from the cache and that they're equal

        }
        [Test]
        public async Task GetLatestContents_ShouldReturnEmptyContent_WhenAppIsNewerThanServerVersion()
        {
            // Arrange //
            byte[] cachedData;

            TextRequestDto _txtRequest = new TextRequestDto() { CurrentVersionNo = "999.99.99" };
            // Act //
            var _txtResponse = await textService.GetLatestVersionAsync(_txtRequest);
            var cacheResponse = cacheManager.TryGetValue(currentFileVersion, out cachedData);

            // Assert //
            Assert.IsTrue(_txtResponse.IsAppVersionUpToDate);
            Assert.IsTrue(_txtResponse.ZipContents is null);
            Assert.IsFalse(_txtResponse.IsZipFileCreated);
            Assert.IsTrue(_txtResponse.IsAppVersionUpToDate);
            Assert.IsFalse(cacheResponse);
        }

        [Test]
        public async Task LastestVersionNumberCache_ContainValue_ReturnsUpToDate()
        {
            cacheManager.Set("LATEST_VERSION", "1.5", mockTextCacheOptions.Object);
            TextService specificTextService = new TextService(loggerMock.Object, cacheManager, mockTextCacheOptions.Object, textOptions, mockMetricLogService.Object);
            TextRequestDto _txtRequest = new TextRequestDto() { CurrentVersionNo = "1.5" };

            // Act //
            var textServiceResponse = await specificTextService.GetLatestVersionAsync(_txtRequest);

            //Assert
            Assert.True(textServiceResponse.IsAppVersionUpToDate);
            Assert.False(textServiceResponse.IsZipFileCreated);
            Assert.Null(textServiceResponse.ZipContents);
        }

        [Test]
        public async Task LatestVersionNumberCache_Is_Set_When_Empty()
        {
            // Arrange //
            string cachedData;
            TextRequestDto _txtRequest = new TextRequestDto() { CurrentVersionNo = "1.5.3" };
            cacheManager.TryGetValue("LATEST_VERSION", out cachedData);
            Assert.True(cachedData == null);

            // Act //
            var textResponse = await textService.GetLatestVersionAsync(_txtRequest);
            var cacheResponse = cacheManager.TryGetValue("LATEST_VERSION", out cachedData);

            // Assert //
            Assert.IsTrue(cacheResponse);
            Assert.IsTrue(cachedData != null);
        }

        [Test]
        public async Task File_Cache_Is_Created_When_Empty()
        {
            // Arrange //
            byte[] cachedData;
            TextRequestDto _txtRequest = new TextRequestDto() { CurrentVersionNo = "0.9" };
            cacheManager.TryGetValue(currentFileVersion, out cachedData);

            Assert.True(cachedData == null);

            // Act //
            var textResponse = await textService.GetLatestVersionAsync(_txtRequest);
            var cacheResponse = cacheManager.TryGetValue(currentFileVersion, out cachedData);

            // Assert //
            Assert.IsTrue(cacheResponse);
            Assert.IsTrue(cachedData != null);
        }

        [Test]
        public async Task Cache_Contains_Expected_Data()
        {
            // Arrange //
            byte[] cachedData;
            cacheManager.Set("LATEST_VERSION", "2.0", mockTextCacheOptions.Object);
            cacheManager.Set("2.0", cacheData, mockTextCacheOptions.Object);
            TextService specificTextService = new TextService(loggerMock.Object, cacheManager, mockTextCacheOptions.Object, textOptions, mockMetricLogService.Object);

            // Act //
            TextRequestDto _txtRequest = new TextRequestDto() { CurrentVersionNo = "0.9" };
            var textResponse = await textService.GetLatestVersionAsync(_txtRequest);

            cacheManager.TryGetValue("2.0", out cachedData);

            Assert.IsTrue(cachedData != null);
            Assert.AreEqual(textResponse.ZipContents, cachedData);
        }

        private string getServerVersion(DirectoryInfo dirInfo)
        {
            var files = dirInfo.GetFiles();
            string sPattern = @"(?![\\_\.])[\d\.\\_]+(?i)(?=.json)"; // This regex is more robust than the one above

            foreach (var file in files)
            {
                var Server_Matches = Regex.Matches(file.Name, sPattern);
                if (Server_Matches.Count == 1)
                {
                    string joined_ServerVersion = string.Join(".", Server_Matches.First().Value);

                    Version versionServer = new Version(joined_ServerVersion);
                    var serverVersion = versionServer.ToString();
                    return serverVersion;
                }
            }

            throw new Exception("could not find files with correct format");
        }
    }
}
