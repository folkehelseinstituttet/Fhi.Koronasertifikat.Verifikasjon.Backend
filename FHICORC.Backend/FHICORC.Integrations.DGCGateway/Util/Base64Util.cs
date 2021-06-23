using System;

namespace FHICORC.Integrations.DGCGateway.Util
{
    public static class Base64Util
    {
        public static byte[] FromString(string base64)
        {
            if (string.IsNullOrEmpty(base64))
            {
                throw new ArgumentException("The base64 parameter must contain data", nameof(base64));
            }

            base64 = base64.Trim();
            base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
            return Convert.FromBase64String(base64);
        }
    }
}
