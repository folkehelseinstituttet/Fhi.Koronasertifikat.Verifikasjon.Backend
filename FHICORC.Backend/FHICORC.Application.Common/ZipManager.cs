using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using FHICORC.Application.Common.Interfaces;

namespace FHICORC.Application.Common
{
    public class ZipManager : IZipManager
    {
        public async Task<Dictionary<string, byte[]>> CreateZipFiles(FileInfo[] files)
        {
            var resultDict = new Dictionary<string, byte[]>();
            foreach (FileInfo file in files)
            {
                resultDict.Add(file.Name, await File.ReadAllBytesAsync(file.FullName));
            }

            return resultDict;
        }
        
        public async Task<byte[]> ZipFiles(Dictionary<string, byte[]> fileDictionary)
        {
            return await Task.Run(() =>
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (ZipArchive archive = new ZipArchive(ms, ZipArchiveMode.Update))
                    {
                        foreach (var file in fileDictionary)
                        {
                            // Create a file with this name
                            ZipArchiveEntry orderEntry = archive.CreateEntry(file.Key); 
                            using (BinaryWriter writer = new BinaryWriter(orderEntry.Open()))
                            {
                                // Write the binary data
                                writer.Write(file.Value);
                            }
                        }
                    }

                    // ZipArchive must be disposed before the MemoryStream has data
                    return ms.ToArray();
                }
            });
        }
    }
}
