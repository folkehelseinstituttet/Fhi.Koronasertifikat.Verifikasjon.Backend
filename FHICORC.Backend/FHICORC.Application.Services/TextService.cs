using Microsoft.Extensions.Logging;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FHICORC.Application.Common.Interfaces;
using FHICORC.Application.Models.CustomExceptions;

namespace FHICORC.Application.Services
{
    public class TextService : ITextService
    {
        private readonly ILogger<TextService> _logger;
        private readonly ICacheManager _cacheManager;
        private readonly TextCacheOptions _textCacheOptions;
        private readonly DirectoryInfo _directorySelected;
        private const string LatestVersionNumberCacheKey = "LATEST_VERSION";
        private readonly IMetricLogService _metricLogService;
        private readonly IZipManager _zipManager;

        public TextService(ILogger<TextService> logger, ICacheManager cacheManager, TextCacheOptions textCacheOptions, TextOptions textOptions,
            IMetricLogService metricLogService, IZipManager zipManager)
        {
            _logger = logger;
            _cacheManager = cacheManager;
            _textCacheOptions = textCacheOptions;
            _directorySelected = new DirectoryInfo(textOptions.TextsDirectory);
            _metricLogService = metricLogService;
            _zipManager = zipManager;
        }

        public async Task<TextResponseDto> GetLatestVersionAsync(TextRequestDto textRequestDto)
        {
            var latestVersion = GetLatestVersionNumber();

            var resultDto = CheckAppVersionUpToDate(latestVersion, textRequestDto.CurrentVersionNo);
            if (resultDto.IsAppVersionUpToDate)
            {
                return resultDto;
            }
            resultDto = await GetLatestVersionFiles();

            return resultDto;
        }

        private string GetLatestVersionNumber()
        {
            if (_cacheManager.TryGetValue(LatestVersionNumberCacheKey, out string cachedFileVersion))
            {
                _logger.LogDebug("Fetched Latest File Version number from cache");
                return cachedFileVersion;
            }
            try
            {
                var files = _directorySelected.GetFiles();
                string sPattern = @"(?![\\_\.])[\d\.\\_]+(?i)(?=.json)"; // This regex is more robust than the one above

                MatchCollection serverMatches;
                foreach (var file in files)
                {
                    serverMatches = Regex.Matches(file.Name, sPattern); // Find the version number in fileName
                    if (serverMatches.Count == 1)
                    {
                        string joinedServerVersion = string.Join(".", serverMatches.First().Value);

                        string versionServer = new Version(joinedServerVersion).ToString();

                        _cacheManager.Set(LatestVersionNumberCacheKey, versionServer, _textCacheOptions);
                        return versionServer;
                    }
                }

                throw new AppDictionaryFileCouldNotBeFoundException("App dictionary file with the right format could not be found.");
            }
            catch (Exception ex)
            {
                if (ex is IndexOutOfRangeException || ex is FileNotFoundException)
                {
                    _logger.LogError(ex, "Specified directory doesn't contain any text files {Directory}", _directorySelected);
                    throw;
                }
                else if (ex is DirectoryNotFoundException)
                {
                    _logger.LogError(ex, "Specified directory doesn't exist {Directory}", _directorySelected);
                    throw;
                }
                else
                {
                    _logger.LogError(ex, "Problem getting version number from textfile");
                    throw;
                }
            }
        }

        private async Task<TextResponseDto> GetLatestVersionFiles()
        {
            var latestVersion = GetLatestVersionNumber();
            var resultDto = new TextResponseDto(false, true);

            if (_cacheManager.TryGetValue(latestVersion, out byte[] cachedData))
            {
                resultDto.ZipContents = cachedData;
                _logger.LogDebug("Texts successfully retrieved from cache, {VersionNo}", latestVersion);
                _metricLogService.AddMetric("TextService_Files_CacheHit", true);
                return resultDto;
            }

            _metricLogService.AddMetric("TextService_Files_CacheHit", false);
            var files = _directorySelected.GetFiles();

            Dictionary<string, byte[]> fileDictionary = await _zipManager.CreateZipFiles(files);
            resultDto = await SetZipContents(fileDictionary);

            _cacheManager.Set(latestVersion, resultDto.ZipContents, _textCacheOptions);
            return resultDto;
        }

        private async Task<TextResponseDto> SetZipContents(Dictionary<string, byte[]> fileDictionary)
        {
            try
            {
                TextResponseDto textResponseDto = new TextResponseDto(false, true)
                {
                    ZipContents = await _zipManager.ZipFiles(fileDictionary)
                };
                _logger.LogDebug("Files successfully fetched from disk and zipped");
                return textResponseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Files were not able to be zipped.");
                throw;
            }
        }
        
        private static bool IsAppVersionUpToDate(string serverVersionString, string appVersionString)
        {
            Version versionApp = new Version(appVersionString);
            Version versionServer = new Version(serverVersionString);
            return versionServer <= versionApp;
        }

        private TextResponseDto CheckAppVersionUpToDate(string ServerFileName, string AppVersion)
        {
            if (IsAppVersionUpToDate(ServerFileName, AppVersion))
            {
                _logger.LogDebug("App is up to date with latest version.");
                return new TextResponseDto(true, false);
            }
            else
            {
                _logger.LogDebug("App is not up to date with latest version.");
                return new TextResponseDto(false, false);
            }
        }
    }
}
