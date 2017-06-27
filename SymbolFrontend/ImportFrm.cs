using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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
            }
        }

        private void I_Newline(object sender, string line)
        {
            if (listBox1.InvokeRequired)
                listBox1.Invoke(new MethodInvoker(() => listBox1.Items.Add($"{DateTime.Now.ToLongTimeString()}: {line}")));
            else
                listBox1.Items.Add($"{DateTime.Now.ToLongTimeString()}: {line}");
        }
    }
}
