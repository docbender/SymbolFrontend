using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolFrontend
{
    class Import
    {
        public delegate void EventNewLine(object sender, string line);
        public event EventNewLine Newline;

        public Task Run(string cimpath, string project, IEnumerable<string> files, bool dynamic)
        {
            return Task.Run(() =>
            {
                foreach (var s in files)
                {
                    var si = new ProcessStartInfo(cimpath + $"\\clie.exe", $"import \"{s}\"{(dynamic?" -y":"")}");
                    si.UseShellExecute = false;
                    si.CreateNoWindow = true;
                    si.RedirectStandardOutput = true;
                    si.WorkingDirectory = project + "\\master";

                    si.EnvironmentVariables.Add("PRCNAM", "SymbolFrontend");
                    si.EnvironmentVariables.Add("PROJECT", project.Substring(project.LastIndexOf("\\")+1) + ".gef");
                    si.EnvironmentVariables.Add("SITE_ROOT", project + "\\");

                    Newline?.Invoke(this, $"Import souboru {Path.GetFileName(s)}...");

                    try
                    {
                        using (var p = Process.Start(si))
                        {
                            using (StreamReader reader = p.StandardOutput)
                            {
                                while (!reader.EndOfStream)
                                {
                                    string result = reader.ReadLine();
                                    Newline?.Invoke(this, result);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Newline?.Invoke(this, ex.Message);
                    }
                }
            });
        }
    }
}
