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
    public partial class DeviceListBrowserFrm : Form
    {
        SymbolTable symbols;

        public DeviceListBrowserFrm()
        {
            InitializeComponent();
        }

        public DeviceListBrowserFrm(SymbolTable symbols):this()
        {        
            this.symbols = symbols;

            listBox1.Items.Clear();
            listBox1.Items.AddRange(symbols.GetTableList().ToArray());
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
                return;

            dataGridView1.DataSource = new BindingSource { DataSource = symbols.GetDeviceLists()[listBox1.SelectedItem.ToString()] };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
