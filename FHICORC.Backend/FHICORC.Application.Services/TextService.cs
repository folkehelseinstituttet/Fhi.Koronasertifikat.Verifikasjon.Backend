using Microsoft.Extensions.Logging;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FHICORC.Application.Common.Interfaces;

namespace FHICORC.Application.Services
{
    public class TextService : ITextService
    {
        private readonly ILogger<TextService> _logger;
        private readonly ICacheManager _cacheManager;
        private readonly TextCacheOptions _textCacheOptions;
        private readonly TextOptions _textOptions;
        private readonly DirectoryInfo _directorySelected;
        private const string LatestVersionNumberCacheKey = "LATEST_VERSION";

        public TextService(ILogger<TextService> logger, ICacheManager cacheManager, TextCacheOptions textCacheOptions, TextOptions textOptions)
        {
            _logger = logger;
            _cacheManager = cacheManager;
            _textCacheOptions = textCacheOptions;
            _textOptions = textOptions;
            _directorySelected = new DirectoryInfo(_textOptions.TextsDirectory);
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
                var fileName = files[0].Name;  // Get version from first file in the Text directory. All files will have same versioning

                string sPattern = @"(?![\\_\.])[\d\.\\_]+(?i)(?=.json)"; // This regex is more robust than the one above

                var serverMatches = Regex.Matches(fileName, sPattern); // Find the version number in fileName
                string joinedServerVersion = string.Join(".", serverMatches.Select(x => x.Value));

                string versionServer = new Version(joinedServerVersion).ToString();

                _cacheManager.Set(LatestVersionNumberCacheKey, versionServer, _textCacheOptions);
                return versionServer;
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
                return resultDto;
            }

            var files = _directorySelected.GetFiles();

            Dictionary<string, byte[]> fileDictionary = await CreateZipFiles(files);
            resultDto = await SetZipContents(fileDictionary);

            _cacheManager.Set(latestVersion, resultDto.ZipContents, _textCacheOptions);
            return resultDto;
        }

        private static async Task<Dictionary<string, byte[]>> CreateZipFiles(FileInfo[] files)
        {
            var resultDict = new Dictionary<string, byte[]>();
            foreach (FileInfo file in files)
            {
                resultDict.Add(file.Name, await File.ReadAllBytesAsync(file.FullName));
            }
            return resultDict;
        }

        private async Task<TextResponseDto> SetZipContents(Dictionary<string, byte[]> fileDictionary)
        {
            try
            {
                TextResponseDto textResponseDto = new TextResponseDto(false, true)
                {
                    ZipContents = await ZipFiles(fileDictionary)
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

        private static async Task<byte[]> ZipFiles(Dictionary<string, byte[]> fileDictionary)
        {
            return await Task.Run(() =>
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (ZipArchive archive = new ZipArchive(ms, ZipArchiveMode.Update))
                    {
                        foreach (var file in fileDictionary)
                        {
                            ZipArchiveEntry orderEntry = archive.CreateEntry(file.Key); //create a file with this name
                            using (BinaryWriter writer = new BinaryWriter(orderEntry.Open()))
                            {
                                writer.Write(file.Value); //write the binary data
                            }
                        }
                    }
                    //ZipArchive must be disposed before the MemoryStream has data
                    return ms.ToArray();
                }
            });
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
