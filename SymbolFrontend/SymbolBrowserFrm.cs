using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SymbolFrontend
{
    public partial class SymbolBrowserFrm : Form
    {
        SymbolTable symbols;
        string searchtext = "";
        int lastRowIndex = 0;

        public SymbolBrowserFrm()
        {
            InitializeComponent();
        }

        public SymbolBrowserFrm(SymbolTable symbols):this()
        {
            this.symbols = symbols;
            symbols.Generate();
            dataGridView1.DataSource = new BindingSource { DataSource = symbols.Symbols };
            label1.Text = $"{dataGridView1.RowCount} symbolů";
            dataGridView1.AutoResizeColumns();
        }

        private void SymbolBrowserFrm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridView1_KeyUp(object sender, KeyEventArgs e)
        {
            if ((!e.Control || e.KeyCode != Keys.F || e.Shift || e.Alt) && (e.Control || e.KeyCode != Keys.F3 || e.Alt))
            {
                e.Handled = false;
                return;
            }

            var b = dataGridView1.SelectedCells;

            if (b != null && b.Count > 0)
            {
                var r = b[0].RowIndex;
                var c = b[0].ColumnIndex;

                if (lastRowIndex > 0)
                    lastRowIndex = r;

                if (e.KeyCode == Keys.F || (e.KeyCode == Keys.F3 && searchtext.Length == 0))
                {
                    var dlg = new SearchFrm();
                    dlg.Input = searchtext;
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

                DataGridViewRow row;

                if (e.KeyCode == Keys.F3 && e.Shift)
                    row = dataGridView1.Rows.Cast<DataGridViewRow>().LastOrDefault(x => x.Cells[c].Value.ToString().IndexOf(searchtext, StringComparison.InvariantCultureIgnoreCase) >= 0 && x.Index < lastRowIndex);
                else
                    row = dataGridView1.Rows.Cast<DataGridViewRow>().FirstOrDefault(x => x.Cells[c].Value.ToString().IndexOf(searchtext, StringComparison.InvariantCultureIgnoreCase) >= 0 && x.Index > lastRowIndex);

                if (row != null)
                {
                    dataGridView1.ClearSelection();
                    dataGridView1.FirstDisplayedScrollingRowIndex = row.Index;
                    row.Cells[c].Selected = true;
                    lastRowIndex = row.Index;
                }
                else
                    lastRowIndex = 0;
            }

            e.Handled = true;
        }
    }
}
