using System;
using Microsoft.Extensions.Caching.Memory;
using FHICORC.Application.Common.Interfaces;
using FHICORC.Application.Models.Options;

namespace FHICORC.Application.Common
{
    public class CacheManager : ICacheManager
    {
        private readonly IMemoryCache _memoryCache;

        public CacheManager(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void Set(string key, object value, CacheOptions cacheOptions)
        {
            MemoryCacheEntryOptions memoryCacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = DateTime.UtcNow.AddMinutes(cacheOptions.AbsoluteExpiration),
                Priority = CacheItemPriority.High,
                SlidingExpiration = TimeSpan.FromMinutes(cacheOptions.SlidingExpiration),
                Size = cacheOptions.CacheSize,
            };

            _memoryCache.Set(key, value, memoryCacheEntryOptions);
        }

        public bool TryGetValue<T>(object key, out T value)
        {
            return _memoryCache.TryGetValue(key, out value);
        }
    }
}