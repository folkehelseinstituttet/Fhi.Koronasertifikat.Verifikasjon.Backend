using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FHICORC.Application.Common.Interfaces;
using FHICORC.Application.Models;
using FHICORC.Application.Models.Options;
using FHICORC.Application.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace FHICORC.Application.Services
{
    public class ValueSetService : IValueSetService
    {
        private readonly ILogger<ValueSetService> _logger;
        private readonly ICacheManager _cacheManager;
        private readonly ValueSetCacheOptions _valueSetCacheOptions;
        private readonly DirectoryInfo _directorySelected;
        private const string ValueSetsCacheKey = "VALUESETS_CACHE_KEY";
        private readonly IMetricLogService _metricLogService;
        private readonly IZipManager _zipManager;

        public ValueSetService(ILogger<ValueSetService> logger, ICacheManager cacheManager, ValueSetCacheOptions valueSetCacheOptions,
            ValueSetOptions valueSetOptions, IMetricLogService metricLogService, IZipManager zipManager)
        {
            _logger = logger;
            _cacheManager = cacheManager;
            _valueSetCacheOptions = valueSetCacheOptions;
            _directorySelected = new DirectoryInfo(valueSetOptions.ValueSetsDirectory);
            _metricLogService = metricLogService;
            _zipManager = zipManager;
        }

        public async Task<ValueSetResponseDto> GetLatestVersionAsync(ValueSetRequestDto valueSetRequestDto)
        {
            if (_cacheManager.TryGetValue(ValueSetsCacheKey, out ValueSetResponseDto cachedData))
            {
                _logger.LogDebug("ValueSets successfully retrieved from cache, {LastUpdated}", cachedData.LastUpdated);
                _metricLogService.AddMetric("ValueSetService_Files_CacheHit", true);
            }
            else
            {
                _metricLogService.AddMetric("ValueSetService_Files_CacheHit", false);
                var files = _directorySelected.GetFiles();
                var lastUpdated = files.Max(fi => fi.LastWriteTime);

                Dictionary<string, byte[]> fileDictionary = await _zipManager.CreateZipFiles(files);
                cachedData = await SetZipContents(fileDictionary, lastUpdated);

                _cacheManager.Set(ValueSetsCacheKey, cachedData, _valueSetCacheOptions);
            }

            var upToDate = valueSetRequestDto.LastFetched != null && valueSetRequestDto.LastFetched.Value.CompareTo(cachedData.LastUpdated) >= 0;
            if (upToDate)
            {
                return new ValueSetResponseDto(true, false);
            }

            return cachedData;
        }
        
        private async Task<ValueSetResponseDto> SetZipContents(Dictionary<string, byte[]> fileDictionary, DateTime lastUpdated)
        {
            // Truncate timestamp to exclude milliseconds (for comparison, as the serialized value does not contain them)
            var lastUpdatedTruncated = new DateTime(lastUpdated.Year, lastUpdated.Month, lastUpdated.Day,
                lastUpdated.Hour, lastUpdated.Minute, lastUpdated.Second, lastUpdated.Kind);

            try
            {
                ValueSetResponseDto valueSetResponseDto = new ValueSetResponseDto(false, true)
                {
                    ZipContents = await _zipManager.ZipFiles(fileDictionary),
                    LastUpdated = lastUpdatedTruncated
                };
                _logger.LogDebug("Files successfully fetched from disk and zipped");
                return valueSetResponseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Files were not able to be zipped.");
                throw;
            }
        }
    }
}
