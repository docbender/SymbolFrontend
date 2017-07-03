using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SymbolFrontend
{
    public partial class ImportFrm : Form
    {
        string cimpath, project;
        string[] files;
        string logFile = "";

        public ImportFrm()
        {
            InitializeComponent();
        }

        public ImportFrm(string cimpath, string project, string[] files)
            : this()
        {
            this.cimpath = cimpath;
            this.project = project;
            this.files = files;
        }

        private async void ImportFrm_Load(object sender, EventArgs e)
        {
            await Import();
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            await Import();
        }

        private async Task Import()
        {
            button2.Enabled = false;
            listBox1.Items.Clear();

            listBox1.Items.Add($"{DateTime.Now.ToLongTimeString()}: Import pointů do projektu {project} zahájen");

            var i = new Import();
            i.Newline += I_Newline;

            try
            {
                await i.Run(cimpath, project, files);
            }
            catch (AggregateException ex)
            {
                MessageBox.Show(string.Join("\n", ex.InnerExceptions.Select(x => x.Message)), "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                button2.Enabled = true;
                listBox1.Items.Add($"{DateTime.Now.ToLongTimeString()}: Import pointů dokončen");

                int errors = 0;
                
                foreach (string line in listBox1.Items)
                {
                    var m = Regex.Match(line, "((\\d*) errors)");

                    if (m.Success)
                    {
                        int e = 0;
                        if (int.TryParse(m.Groups[2].Value, out e))
                            errors += e;
                    }
                }

                if (errors > 0)
                {
                    label1.Text = $"Celkem nalezeno {errors} chyb při importu";
                    label1.ForeColor = Color.Red;
                }
                else
                {
                    label1.Text = $"Import proběhl bez chyb";
                    label1.ForeColor = Color.White;
                }

                logFile = "";
                foreach (string line in listBox1.Items)
                {
                    var m = Regex.Match(line, @"(LOG_PATH:(.*\.log))");

                    if (m.Success)
                    {
                        logFile = m.Groups[2].Value;
                        break;                    
                    }
                }

                button3.Enabled = logFile.Length > 0;
            }
        }

        private void I_Newline(object sender, string line)
        {
            if (listBox1.InvokeRequired)
                listBox1.Invoke(new MethodInvoker(() => listBox1.Items.Add($"{DateTime.Now.ToLongTimeString()}: {line}")));
            else
                listBox1.Items.Add($"{DateTime.Now.ToLongTimeString()}: {line}");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            ShowLog();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
                return;

            var m = Regex.Match(listBox1.SelectedItem.ToString(), @"(LOG_PATH:(.*\.log))");

            if (m.Success)
                logFile = m.Groups[2].Value;
            else
                logFile = "";

            button3.Enabled = logFile.Length > 0;
        }

        void ShowLog()
        {
            if (logFile.Length == 0)
                return;

            var file = $"{project}\\log\\{logFile}";

            if (!File.Exists(file))
            {
                MessageBox.Show($"Soubor {file} neexistuje", "Upozornění", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (var p = Process.Start(file))
                {
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Chyba při otevírání souboru {file}. {ex.Message}", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
