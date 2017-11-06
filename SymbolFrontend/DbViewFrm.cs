using DotNetSiemensPLCToolBoxLibrary.DataTypes;
using DotNetSiemensPLCToolBoxLibrary.DataTypes.Blocks.Step7V5;
using DotNetSiemensPLCToolBoxLibrary.Projectfiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SymbolFrontend
{
    public partial class DbViewFrm : Form
    {
        DbClass db = null;
        string project;
        string searchtext = "";
        int lastRowIndex = 0;

        private DbViewFrm()
        {
            InitializeComponent();
        }

        public DbViewFrm(DbClass db, string project)
            : this()
        {
            this.db = db;
            this.project = project;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void DbViewFrm_Load(object sender, EventArgs e)
        {
            Text += $" - {db.Name}";

            GetDataBlock();
        }

        private void GetDataBlock()
        {
            listBox1.Items.Clear();

            if (!File.Exists(project))
            {
                MessageBox.Show($"Projekt {project} neexistuje", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Project tmp = Projects.LoadProject(project, false);
            var prj = tmp as Step7ProjectV5;

            if (prj == null)
            {
                MessageBox.Show($"Projekt {project} se nepodařilo otevřít", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            var dbData = prj.BlocksOfflineFolders.FirstOrDefault(x => x.StructuredFolderName.Equals(db.Path))
                ?.BlockInfos?.Where(z => z.BlockType == PLCBlockType.DB)
                ?.Select(c => c.GetBlock() as S7DataBlock)
                ?.FirstOrDefault(v => v.BlockName.Equals(db.Name));
            if (dbData == null)
            {
                MessageBox.Show($"Datablok {db.Name} se nepodařilo v projektu {project} najít", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
                return;
            }

            //label6.Text = dbData.BlockNumber.ToString();
            //label7.Text = dbData.BlockName;
            //label11.Text = dbData.CodeSize.ToString();
            //label13.Text = dbData.LastCodeChange.ToString("yy-MM-dd HH:mm:ss");

            listBox1.Items.AddRange((dbData).ToString().Split(new char[] { '\n' }));
        }

        private void listBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if ((!e.Control || e.KeyCode != Keys.F || e.Shift || e.Alt) 
                && (e.Control || e.KeyCode != Keys.F3 || e.Alt)
                && (!e.Control || e.KeyCode != Keys.C || e.Shift || e.Alt))
            {
                e.Handled = false;
                return;
            }

            //copy
            if(e.Control && e.KeyCode == Keys.C)
            {
                Clipboard.SetText(listBox1.SelectedItem?.ToString() ?? "");

                e.Handled = true;
                return;
            }

            var r = listBox1.SelectedIndex;

            if (lastRowIndex > 0)
                lastRowIndex = r;

            if (e.KeyCode == Keys.F || (e.KeyCode == Keys.F3 && searchtext.Length == 0))
            {
                var dlg = new SearchFrm();
                dlg.Input = searchtext;
                dlg.StartPosition = FormStartPosition.CenterParent;
                //show input dialog
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    searchtext = dlg.Input;
                }
                else
                {
                    e.Handled = true;
                    return;
                }
            }

            string row;

            if (e.KeyCode == Keys.F3 && e.Shift)
                row = listBox1.Items.Cast<string>().LastOrDefault(x => x.IndexOf(searchtext, StringComparison.InvariantCultureIgnoreCase) >= 0 && listBox1.Items.IndexOf(x) < lastRowIndex);            
            else
                row = listBox1.Items.Cast<string>().FirstOrDefault(x => x.IndexOf(searchtext, StringComparison.InvariantCultureIgnoreCase) >= 0 && listBox1.Items.IndexOf(x) > lastRowIndex);

            if (row != null)
            {
                listBox1.ClearSelected();
                listBox1.SelectedIndex = listBox1.Items.IndexOf(row);
                lastRowIndex = listBox1.SelectedIndex;
            }
            else
                lastRowIndex = 0;        

            e.Handled = true;
        }
}
}
