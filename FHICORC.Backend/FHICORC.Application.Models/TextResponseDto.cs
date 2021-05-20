namespace FHICORC.Application.Models
{
    public class TextResponseDto
    {
        public TextResponseDto(bool isAppVersionUpToDate, bool isZipFileCreated)
        {
            this.IsAppVersionUpToDate = isAppVersionUpToDate;
            this.IsZipFileCreated = isZipFileCreated;
        }
        public bool IsAppVersionUpToDate { get; set; }

        public bool IsZipFileCreated { get; set; }

        public byte[] ZipContents { get; set; }

    }
}
