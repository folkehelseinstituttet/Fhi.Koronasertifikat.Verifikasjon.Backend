using Newtonsoft.Json;
using System.Collections.Generic;

namespace FHICORC.Application.Models
{
    public class PublicKeyJson
    {
        [JsonProperty(PropertyName = "kvp")]
        public List<Kvp> kvpList;
    }

    public class Kvp
    {
        [JsonProperty(PropertyName = "kid")]
        public string kid;
        [JsonProperty(PropertyName = "pk")]
        public string pk;
    }
}