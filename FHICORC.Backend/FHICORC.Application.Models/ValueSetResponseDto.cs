using System;

namespace FHICORC.Application.Models
{
    public class ValueSetResponseDto
    {
        public ValueSetResponseDto(bool isAppVersionUpToDate, bool isZipFileCreated)
        {
            this.IsAppVersionUpToDate = isAppVersionUpToDate;
            this.IsZipFileCreated = isZipFileCreated;
        }
        public bool IsAppVersionUpToDate { get; set; }

        public bool IsZipFileCreated { get; set; }

        public DateTimeOffset LastUpdated { get; set; }

        public byte[] ZipContents { get; set; }
    }
}
