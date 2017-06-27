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
    public partial class LogFrm : Form
    {
        string[] messages;

        public LogFrm()
        {
            InitializeComponent();
        }

        public LogFrm(string[] messages)
            : this()
        {
            this.messages = messages;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void LogFrm_Load(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            listBox1.Items.AddRange(messages);
            label1.Text = $"{messages.Length} záznamů";
        } 
    }
}
