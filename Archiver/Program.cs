using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Archiver
{
    class Program
    {
        static void Main(string[] args)
        {
            string logFile = @"D:\Zalohy\Dropbox\Chlmec\backup.log";
            List<string> log = new List<string>();


            string step7Folder = @"D:\PLC\TPCH_S7H";
            string cimFolder = @"C:\TPCH";
            string step7ArchiveFolder = @"D:\Zalohy\Dropbox\Chlmec\PLC";
            string cimArchiveFolder = @"D:\Zalohy\Dropbox\Chlmec\SCADA";
            //string archivePath = $"{archiveFolder}\\{Path.GetFileName(step7Folder)}_{DateTime.Now.ToString("yyyyMMdd-HHmmss")}.zip";
            string step7ArchivePath = $"{step7ArchiveFolder}\\{Path.GetFileName(step7Folder)}.zip";
            string cimArchivePath = $"{cimArchiveFolder}\\{Path.GetFileName(cimFolder)}.zip";

            log.Add($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Spoustim zalohu pro {step7Folder} a {cimFolder}");
            Task<long> t1 = null, t2 = null;

            t1 = Task<long>.Factory.StartNew(() => ZipArchive.ArchiveFolder(step7Folder, step7ArchivePath));
            t2 = Task<long>.Factory.StartNew(() => ZipArchive.ArchiveFolder(cimFolder, cimArchivePath, new string[] { "\\.Running$", @"\\log\\" }));

            try
            {

                Task.WaitAll(new Task[] { t1, t2 });
            }
            catch (AggregateException ex)
            {
                for (int i = 0; i < ex.InnerExceptions.Count; i++)
                {
                    Console.WriteLine("!!!---{0}", ex.InnerExceptions[i].ToString());
                    log.Add($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Chyba: {ex.InnerExceptions[i].Message}");
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("!!!---{0}", ex.ToString());
                log.Add($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Chyba: {ex.Message}");
            }

            if(t1 != null && t1.Status != TaskStatus.Faulted)
                log.Add($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Provedena zaloha souboru {step7ArchivePath} s velikosti {t1.Result}B");
            else
                log.Add($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Nastala chyba pri zalohovani do souboru {step7ArchivePath}");

            if (t2!=null && t2.Status != TaskStatus.Faulted)
                log.Add($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Provedena zaloha souboru {cimArchivePath} s velikosti {t2.Result}B");
            else
                log.Add($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Nastala chyba pri zalohovani do souboru {cimArchivePath}");

            log.Add($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} Zaloha dokoncena");

            try
            {
                File.WriteAllLines(logFile, log);
            }
            catch(Exception ex)
            {
                Console.WriteLine("!!!---Chyba pri zapisu do logu {0}", ex.ToString());
            }

            Console.WriteLine("Hotovo");
        }
    }
}
