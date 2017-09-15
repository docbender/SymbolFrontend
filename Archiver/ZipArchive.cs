using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;
using System.Text.RegularExpressions;

namespace Archiver
{    
    class ZipArchive
    {
        public static long ArchiveFolder(string path, string targetFile)
        {
            return ArchiveFolder(path, targetFile, null);
        }
        public static long ArchiveFolder(string path, string targetFile, string[] excluedeFileMask)
        {
            if (!Directory.Exists(path))
            {
                throw new DirectoryNotFoundException();
            }

            ZipOutputStream zip = new ZipOutputStream(File.Create(targetFile));
            zip.SetLevel(9);
            ZipFolder(path, Path.GetFileName(path), path, zip, excluedeFileMask);
            zip.Finish();
            var size = zip.Length;
            zip.Close();

            return size;
        }

        public static void ZipFolder(string RootFolderPath, string RootFolderName, string CurrentFolderPath, ZipOutputStream zStream, string[] excluedeFileMask)
        {
            string[] SubFolders = Directory.GetDirectories(CurrentFolderPath);

            foreach (string Folder in SubFolders)
                ZipFolder(RootFolderPath, RootFolderName, Folder, zStream, excluedeFileMask);
            
            string relativePath = CurrentFolderPath.Substring(RootFolderPath.Length - RootFolderName.Length - 1) + "/";

            if (relativePath.Length > 1)
            {
                ZipEntry dirEntry;

                dirEntry = new ZipEntry(relativePath);
                dirEntry.DateTime = DateTime.Now;
            }

            foreach (string file in Directory.GetFiles(CurrentFolderPath))
            {                
                if(excluedeFileMask != null)
                {
                    bool match = false;
                    foreach (var m in excluedeFileMask)
                    {
                        if(Regex.IsMatch(file, m))
                        {
                            match = true;
                            break;
                        }
                    }

                    if (match)
                        continue;
                }

                AddFileToZip(zStream, relativePath, file);
            }
        }

        private static void AddFileToZip(ZipOutputStream zStream, string relativePath, string file)
        {
            byte[] buffer = new byte[4096];
            string fileRelativePath = (relativePath.Length > 1 ? relativePath : string.Empty) + Path.GetFileName(file);
            ZipEntry entry = new ZipEntry(fileRelativePath);
            
            entry.DateTime = File.GetLastWriteTime(file);
            zStream.PutNextEntry(entry);

            using (FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                int sourceBytes;

                do
                {
                    sourceBytes = fs.Read(buffer, 0, buffer.Length);
                    zStream.Write(buffer, 0, sourceBytes);
                } while (sourceBytes > 0);
            }
        }
    }
}
