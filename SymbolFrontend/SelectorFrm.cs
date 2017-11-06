using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SymbolFrontend
{
    public partial class SelectorFrm : Form
    {
        IEnumerable<string> values;

        protected SelectorFrm()
        {
            InitializeComponent();
        }

        public SelectorFrm(IEnumerable<string> values, IEnumerable<string> checkedValues) 
            : this()
        {
            checkedListBox1.Items.Clear();
            this.values = values;

            if (checkedValues == null)
            {
                foreach (var v in values)
                    checkedListBox1.Items.Add(v, true);
            }
            else
            {
                foreach (var v in values)
                    checkedListBox1.Items.Add(v, checkedValues.Contains(v));
            }
        }

        public IEnumerable<string> Selected
        {
            get
            {
                if (checkedListBox1.CheckedItems.Count == 0)
                    return null;

                var list = new List<string>();
                foreach(var s in checkedListBox1.CheckedItems)
                {
                    list.Add(s.ToString());
                }

                return list;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void checkedListBox1_Format(object sender, ListControlConvertEventArgs e)
        {
            if(e.Value is string)
            {
                var i = (e.Value as string).LastIndexOf('\\');
                if (i >= 0 && (e.Value as string).Length > i)
                    e.Value = (e.Value as string).Substring(i+1);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for(int i=0;i<checkedListBox1.Items.Count;i++)
            {
                checkedListBox1.SetItemChecked(i, true);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, false);
            }
        }
    }
}
