﻿using DotNetSiemensPLCToolBoxLibrary.DataTypes;
using DotNetSiemensPLCToolBoxLibrary.DataTypes.Blocks;
using DotNetSiemensPLCToolBoxLibrary.DataTypes.Blocks.Step7V5;
using DotNetSiemensPLCToolBoxLibrary.DataTypes.Projectfolders.Step7V5;
using DotNetSiemensPLCToolBoxLibrary.Projectfiles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using System.IO;

namespace SymbolFrontend
{
    public partial class S7BrowseFrm : Form
    {
        public string ProjectPath { get; private set; }
        public S7BrowseFrm()
        {
            InitializeComponent();

            listBox4.Items.Clear();

            ProjectPath = Properties.Settings.Default.projectpath;
            label2.Text = ProjectPath;

            if (ProjectPath.Length > 0)
                LoadProject(ProjectPath);

            if (Properties.Settings.Default.projectfolder != null && Properties.Settings.Default.projectfolder.Length > 0)
            {
                for (int i=0;i<listBox1.Items.Count;i++)
                {
                    if ((listBox1.Items[i] as BlocksOfflineFolder).StructuredFolderName == Properties.Settings.Default.projectfolder)
                    {
                        listBox1.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        public S7BrowseFrm(DbCollection datablocks)
            : this()
        {
            if (datablocks != null)
            {
                listBox4.Items.AddRange(datablocks.ToArray());
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Filter = "Vse (*.zip, *.s7p, *.ap11)|*.s7p;*.zip;*.ap11|Step7 V5.5 Project|*.s7p;*.s7l|Zipped Step5/Step7 Project|*.zip|TIA-Portal Project|*.ap11";

            var ret = op.ShowDialog();
            if (ret == DialogResult.OK)
            {
                ProjectPath = op.FileName;
                label2.Text = ProjectPath;
                LoadProject(ProjectPath);
            }
        }

        private void LoadProject(string projectPath)
        {
            if (!File.Exists(projectPath))
            {
                MessageBox.Show($"Projekt {projectPath} neexistuje", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            Project tmp = Projects.LoadProject(projectPath, false);
            var prj = tmp as Step7ProjectV5;

            if (prj == null)
            {
                MessageBox.Show($"Projekt {projectPath} se nepoddařilo otevřít", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            if (prj != null)
            {
                listBox1.Items.Clear();
                listBox1.Items.AddRange(prj.BlocksOfflineFolders.ToArray());
            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
                return;

            listBox2.Items.Clear();
            listBox3.Items.Clear();
            
            if(listBox4.Items.Count > 0 && listBox1.SelectedItem is BlocksOfflineFolder)
            {
                foreach (var i in listBox4.Items)
                    (i as DbClass).Path = (listBox1.SelectedItem as BlocksOfflineFolder).StructuredFolderName;
            }
            else
                listBox4.Items.Clear();

                var db = listBox1.SelectedItem as BlocksOfflineFolder;
            if (db != null)
            {
                if (checkBox1.Checked)
                    listBox2.Items.AddRange(db.BlockInfos.Where(x => x.BlockType == PLCBlockType.DB || x.BlockType == PLCBlockType.FB || x.BlockType == PLCBlockType.FC || x.BlockType == PLCBlockType.OB || x.BlockType == PLCBlockType.UDT || x.BlockType == PLCBlockType.VAT).OrderBy(x => x.BlockType).ToArray());
                else
                    listBox2.Items.AddRange(db.BlockInfos.Where(x => x.BlockType == PLCBlockType.DB).ToArray());
            }
        }

        private void S7BrowseFrm_Load(object sender, EventArgs e)
        {

        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox3.Items.Clear();

            if (listBox2.SelectedItem == null)
            {
                label6.Text = "";
                label7.Text = "";
                label11.Text = "";
                label13.Text = "";
                return;
            }

            var blockinfo = (listBox2.SelectedItem as ProjectBlockInfo);

            var block = blockinfo.GetBlock();
            label6.Text = block.BlockNumber.ToString();
            label7.Text = block.BlockName;            

            if (blockinfo.BlockType == PLCBlockType.DB)
            {
                var db = blockinfo.GetBlock() as S7DataBlock;

                label11.Text = db.CodeSize.ToString();
                label13.Text = db.LastCodeChange.ToString("yy-MM-dd HH:mm:ss");

                listBox3.Items.AddRange(db.ToString().Split(new char[] { '\n' }));
            }
            else if (blockinfo.BlockType == PLCBlockType.FB || blockinfo.BlockType == PLCBlockType.FC || blockinfo.BlockType == PLCBlockType.OB)
            {
                var fb = blockinfo.GetBlock() as S7FunctionBlock;

                label11.Text = fb.CodeSize.ToString();
                label13.Text = fb.LastCodeChange.ToString("yy-MM-dd HH:mm:ss");

                listBox3.Items.AddRange(fb.ToString().Split(new char[] { '\n' }));
            }
            else if (blockinfo.BlockType == PLCBlockType.VAT)
            {
                var fb = blockinfo.GetBlock() as S7VATBlock;

                label11.Text = fb.CodeSize.ToString();
                label13.Text = fb.LastCodeChange.ToString("yy-MM-dd HH:mm:ss");

                listBox3.Items.AddRange(fb.ToString().Split(new char[] { '\n' }));
            }
            else if (blockinfo.BlockType == PLCBlockType.UDT)
            {
                var fb = blockinfo.GetBlock() as S7DataBlock;

                label11.Text = fb.CodeSize.ToString();
                label13.Text = fb.LastCodeChange.ToString("yy-MM-dd HH:mm:ss");

                listBox3.Items.AddRange(fb.ToString().Split(new char[] { '\n' }));
            }
        }

        public Nullable<int> SelectedDbNumber
        {
            get
            {
                if (listBox2.SelectedItem == null)
                    return null;
                var db = (listBox2.SelectedItem as ProjectBlockInfo).GetBlock() as S7DataBlock;
                return db.BlockNumber;
            }
        }

        public string ProjectFolder
        {
            get
            {
                return (listBox1.SelectedItem as BlocksOfflineFolder)?.StructuredFolderName;
            }
        }

        public DbCollection SelectedDataBlocks
        {
            get
            {
                var l = new DbCollection();


                foreach (var i in listBox4.Items)
                    l.Add(i as DbClass);

                return l;
            }
            set
            {
                listBox4.Items.AddRange(value.ToArray());
            }
        }

        private void listBox2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox2.SelectedItem == null || (listBox2.SelectedItem as ProjectBlockInfo).BlockType != PLCBlockType.DB)
                return;
            var db = (listBox2.SelectedItem as ProjectBlockInfo).GetBlock() as S7DataBlock;

            var dbc = new DbClass();
            dbc.CopyFrom(db);

            bool isIn = false;
            foreach (DbClass v in listBox4.Items)
            {
                if (v.Equals(dbc))
                {
                    isIn = true;
                    break;
                }

            }
            if (!isIn)
                listBox4.Items.Add(dbc);
        }

        private void listBox4_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox4.SelectedItem == null)
                return;

            listBox4.Items.Remove(listBox4.SelectedItem);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
                return;

            listBox2.Items.Clear();

            var db = listBox1.SelectedItem as BlocksOfflineFolder;
            if (db != null)
            {
                if (checkBox1.Checked)
                    listBox2.Items.AddRange(db.BlockInfos.Where(x => x.BlockType == PLCBlockType.DB || x.BlockType == PLCBlockType.FB || x.BlockType == PLCBlockType.FC || x.BlockType == PLCBlockType.OB || x.BlockType == PLCBlockType.UDT || x.BlockType == PLCBlockType.VAT).OrderBy(x => x.BlockType).ToArray());
                else
                    listBox2.Items.AddRange(db.BlockInfos.Where(x => x.BlockType == PLCBlockType.DB).ToArray());
            }
        }
    }
}
