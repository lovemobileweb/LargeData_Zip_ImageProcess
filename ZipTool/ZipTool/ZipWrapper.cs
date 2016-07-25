using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;

namespace ZipTool
{
    class ZipWrapper
    {
        /// <summary>
        /// zip addFile to path
        /// </summary>
        public static bool Zip(string path, string addFile, int chunkSize = 4096)
        {
            try
            {
                long chunks = 0;
                using (FileStream zipToOpen = new FileStream(path, FileMode.Create))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                    {
                        ZipArchiveEntry entry = archive.CreateEntry(Path.GetFileName(addFile));
                        using (Stream writer = entry.Open())
                        {
                            using (FileStream reader = new FileStream(addFile, FileMode.Open))
                            {
                                byte[] chunk = new byte[chunkSize];
                                int readSize = 0;
                                while ((readSize = reader.Read(chunk, 0, chunkSize)) > 0)
                                {
                                    writer.Write(chunk, 0, readSize);
                                    chunks++;
                                    Utilities.Log(string.Format("."), false);
                                }
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Utilities.Log(string.Format("Error [Zip] {0}", ex.Message));
            }
            return false;
        }

        /// <summary>
        /// unzip path to extractPath
        /// </summary>
        public static bool Unzip(string path, string extractPath, int chunkSize = 4096)
        {
            try
            {
                long chunks = 0;
                using (FileStream zipToOpen = new FileStream(path, FileMode.Open))
                {
                    using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Read))
                    {
                        ZipArchiveEntry entry = archive.Entries[0];
                        using (Stream reader = entry.Open())
                        {
                            using (FileStream writer = new FileStream(extractPath, FileMode.Create))
                            {
                                byte[] chunk = new byte[chunkSize];
                                int readSize = 0;
                                while ((readSize = reader.Read(chunk, 0, chunkSize)) > 0)
                                {
                                    writer.Write(chunk, 0, readSize);
                                    chunks++;
                                    Utilities.Log(string.Format("."), false);
                                }
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Utilities.Log(string.Format("Error [Unzip] {0}", ex.Message));
            }
            return false;
        }
    }
}
