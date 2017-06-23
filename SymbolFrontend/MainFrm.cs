﻿using System;
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
    public partial class MainFrm : Form
    {
        access db = new access();
        SymbolTable symbols = new SymbolTable();
        DefinitionCollection definitions = new DefinitionCollection();
        C32n pointStructures = new C32n();
        public MainFrm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var s = db.GetTableList();
                MessageBox.Show(string.Join(";", s));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            button2.Enabled = false;

            try
            {
                var dL = checkBox2.Checked ? symbols.GetDeviceLists() : db.GetDeviceLists();
                if (dL == null)
                {
                    MessageBox.Show("Nepodařilo se získat seznam zařízení. Akce zastavena.", "Upozornění", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                await assigner.Run(dL);

                MessageBox.Show("Hotovo", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (AggregateException ex)
            {
                MessageBox.Show(string.Join("\n", ex.InnerExceptions.Select(x => x.Message)), "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Join("\n", ex.Message), "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                button2.Enabled = true;
                Cursor.Current = Cursors.Default;
            }
        }

        private async void MainFrm_Load(object sender, EventArgs e)
        {
            checkBox1.DataBindings.Add("Checked", Properties.Settings.Default, "pointsFromProject");
            checkBox2.DataBindings.Add("Checked", Properties.Settings.Default, "screensFromProject");

            AppLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(access)).Location);
            label5.Text = Properties.Settings.Default.projectpath;
            label7.Text = Properties.Settings.Default.projectfolder;
            listBox2.Items.Clear();

            LoadDb();

            if (!Directory.Exists(Path.GetDirectoryName(DbLocation)))
                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(DbLocation));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

            if (File.Exists(DbLocation))
                try
                {
                    SelectedDb = DbCollection.Deserialize(File.ReadAllText(DbLocation));
                    listBox2.Items.Clear();
                    listBox2.Items.AddRange(SelectedDb.ToArray());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

            await LoadPointDefinitions();
        }

        private void LoadDb()
        {
            label2.Text = db.Location;
            listBox1.Items.Clear();

            try
            {
                var s = db.GetTableList();

                listBox1.Items.AddRange(s.ToArray());

                label4.Text = "Připravena";
            }
            catch (Exception ex)
            {
                label4.Text = ex.ToString();
                MessageBox.Show(ex.ToString());
            }
        }

        string AppLocation
        {
            get; set;
        }

        string DbLocation
        {
            get
            {
                return AppLocation + @"\config\datablocks.json";
            }
        }

        DbCollection SelectedDb
        {
            get; set;
        } = null;



        private void button1_Click_1(object sender, EventArgs e)
        {
            var s = new S7BrowseFrm(SelectedDb);

            if (s.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.projectpath = s.ProjectPath;
                Properties.Settings.Default.projectfolder = s.ProjectFolder;
                Properties.Settings.Default.Save();
                label5.Text = Properties.Settings.Default.projectpath;
                label7.Text = Properties.Settings.Default.projectfolder;

                listBox2.Items.Clear();
                listBox2.Items.AddRange(s.SelectedDataBlocks.ToArray());
                SelectedDb = s.SelectedDataBlocks;

                File.WriteAllText(DbLocation, s.SelectedDataBlocks.Serialize());

                symbols.Generate();
            }
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            var d = new OpenFileDialog();

            d.Filter = "Access db|*.accdb|Vse (*.*)|*.*";

            if (db.Location.Length != 0)
                d.FileName = db.Location;

            if (d.ShowDialog() == DialogResult.OK)
            {
                db.Location = d.FileName;
                LoadDb();
            }
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            await LoadPointDefinitions();
        }

        async Task LoadPointDefinitions()
        {
            listBox3.Items.Clear();

            try
            {
                await definitions.Load();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            listBox3.Items.AddRange(definitions.Values.ToArray());

            pointStructures.Load();
        }

        private void listBox3_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }

        private async void button5_Click(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            button5.Enabled = false;

            try
            {
                var dL = checkBox1.Checked ? symbols.GetDeviceLists() : db.GetDeviceLists();
                if (dL == null)
                {
                    MessageBox.Show("Nepodařilo se získat seznam zařízení. Generování zastaveno.", "Upozornění", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                await PointGeneratorCimplicity.Run(SelectedDb, definitions, pointStructures, dL);
            }
            catch (AggregateException ex)
            {
                MessageBox.Show(string.Join("\n", ex.InnerExceptions.Select(x => x.Message)), "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Join("\n", ex.Message), "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            button5.Enabled = true;
            Cursor.Current = Cursors.Default;
        }

        private void MainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            symbols.Generate();
        }
    }
}