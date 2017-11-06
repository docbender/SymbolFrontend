using DotNetSiemensPLCToolBoxLibrary.DataTypes;
using DotNetSiemensPLCToolBoxLibrary.DataTypes.Blocks.Step7V5;
using DotNetSiemensPLCToolBoxLibrary.Projectfiles;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        access db = new access();
        SymbolTable symbols = new SymbolTable();
        DefinitionCollection definitions = new DefinitionCollection();
        C32n pointStructures = new C32n();
        public IEnumerable<string> LastImport { get; protected set; }
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
                label4.Text = ex.Message;
                //MessageBox.Show(ex.ToString());
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
            catch (Exception ex)
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

            Hierarchy hierarchy = LogManager.GetRepository() as Hierarchy;
            MemoryAppender mappender = hierarchy.Root.GetAppender("MemoryAppender") as MemoryAppender;

            try
            {
                var dL = checkBox1.Checked ? symbols.GetDeviceLists() : db.GetDeviceLists();
                if (dL == null)
                {
                    MessageBox.Show("Nepodařilo se získat seznam zařízení. Generování zastaveno.", "Upozornění", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                mappender.Clear();


                if (!RefreshDatablock(SelectedDb))
                {
                    if (MessageBox.Show($"Chyba při provádění refreshe databloků z projektu. Pokračovat s offline daty?", "Dotaz", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
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
            finally
            {
                button5.Enabled = true;
                Cursor.Current = Cursors.Default;
            }

            var ev = mappender.PopAllEvents();
            if (ev != null && ev.Length > 0)
            {
                if (MessageBox.Show("Během generování se vyskytly chyby. Chceš je zobrazit?", "Upozornění", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    var dlg = new LogFrm(ev.OrderBy(x => x.Properties["datablock"]).Select(x => x.RenderedMessage).ToArray());
                    dlg.Show(this);
                }
            }
        }

        static bool RefreshDatablock(DbCollection datablocks)
        {
            string project = Properties.Settings.Default.projectpath;

            if (!File.Exists(project))
            {
                MessageBox.Show($"Projekt {project} neexistuje", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            Project tmp = Projects.LoadProject(project, false);
            var prj = tmp as Step7ProjectV5;

            if (prj == null)
            {
                MessageBox.Show($"Projekt {project} se nepodařilo otevřít", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            foreach (var db in datablocks)
            {
                var dbData = prj.BlocksOfflineFolders.FirstOrDefault(x => x.StructuredFolderName.Equals(Properties.Settings.Default.projectfolder))
                ?.BlockInfos?.Where(z => z.BlockType == PLCBlockType.DB)
                ?.Select(c => c.GetBlock() as S7DataBlock)
                ?.FirstOrDefault(v => v.BlockName.Equals(db.Name));
                if (dbData == null)
                {
                    MessageBox.Show($"Datablok {db.Name} se nepodařilo v projektu {project} najít", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                db.CopyFrom(dbData);
            }
            return true;
        }

        private void MainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();
            File.WriteAllText(DbLocation, SelectedDb.Serialize());
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var dlg = new SymbolBrowserFrm(symbols);
            dlg.Show();
        }

        private void button9_Click(object sender, EventArgs e)
        {
            var dlg = new DeviceListBrowserFrm(symbols);
            dlg.Show();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(PointGeneratorCimplicity.Location))
            {
                try
                {
                    Directory.CreateDirectory(PointGeneratorCimplicity.Location);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            try
            {
                Process.Start(PointGeneratorCimplicity.Location);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            ImportPoints(true, checkBox3.Checked);
        }

        private void ImportPoints(bool all, bool dynamic)
        {
            if (!Directory.Exists(PointGeneratorCimplicity.Location))
            {
                MessageBox.Show($"Cesta k pointům nenalezena ({PointGeneratorCimplicity.Location})", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                var files = Directory.EnumerateFiles(PointGeneratorCimplicity.Location, "*.txt");

                if (files == null || files.Count() == 0)
                {
                    MessageBox.Show($"Adresář pointů neobsahuje žádné txt soubory", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var cimpath = Environment.GetEnvironmentVariable("CIMPATH");

                if (!Directory.Exists(cimpath))
                {
                    MessageBox.Show($"Instalace Cimplicity nenalezena ({cimpath})", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var dlg = new ImportFrm(cimpath, "C:\\TPCH", files.ToArray(), all, dynamic, LastImport);
                dlg.ShowDialog(this);

                LastImport = dlg.LastImport;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            ImportPoints(false, checkBox3.Checked);
        }

        private void listBox2_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (listBox2.SelectedItem == null)
                return;

            var dlg = new DbViewFrm(listBox2.SelectedItem as DbClass, Properties.Settings.Default.projectpath);
            dlg.Show();
        }


    }
}
