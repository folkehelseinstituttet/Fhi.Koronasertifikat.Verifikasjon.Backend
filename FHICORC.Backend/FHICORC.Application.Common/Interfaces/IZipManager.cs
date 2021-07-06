using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FHICORC.Application.Common.Interfaces
{
    public interface IZipManager
    {
        Task<Dictionary<string, byte[]>> CreateZipFiles(FileInfo[] files);
        Task<byte[]> ZipFiles(Dictionary<string, byte[]> fileDictionary);
    }
}
