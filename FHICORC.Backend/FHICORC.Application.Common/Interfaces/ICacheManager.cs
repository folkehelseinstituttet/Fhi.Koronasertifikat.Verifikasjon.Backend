using FHICORC.Application.Models.Options;

namespace FHICORC.Application.Common.Interfaces
{
    public interface ICacheManager
    {
        void Set(string key, object value, CacheOptions cacheOptions);
        bool TryGetValue<T>(object key, out T value);
    }
}