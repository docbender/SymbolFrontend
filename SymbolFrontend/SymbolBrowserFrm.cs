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

        public SymbolBrowserFrm()
        {
            InitializeComponent();
        }

        public SymbolBrowserFrm(SymbolTable symbols):this()
        {
            this.symbols = symbols;
            symbols.Generate();
            dataGridView1.DataSource = new BindingSource { DataSource = symbols.Symbols };
        }

        private void SymbolBrowserFrm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
