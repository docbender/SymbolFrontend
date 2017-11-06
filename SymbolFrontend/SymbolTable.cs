using DotNetSiemensPLCToolBoxLibrary.DataTypes.Projectfolders;
using DotNetSiemensPLCToolBoxLibrary.Projectfiles;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SymbolFrontend
{
    public class SymbolTable : ISymbolData
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Dictionary<string, DeviceCollection> tables = null;


        public Dictionary<string, DeviceCollection> GetDeviceLists()
        {
            if (tables == null)
                RetriveTables();

            return tables;
        }

        public List<string> GetTableList()
        {
            if (tables == null)
                RetriveTables();

            return tables.Keys.ToList();
        }

        public void Generate()
        {
            RetriveTables();
        }

        void RetriveTables()
        {
            tables = null;

            if (!File.Exists(Properties.Settings.Default.projectpath))
                return;

            Project tmp = Projects.LoadProject(Properties.Settings.Default.projectpath, false);
            var prj = tmp as Step7ProjectV5;

            if (prj == null)
                return;

            if (prj.BlocksOfflineFolders.FirstOrDefault(x => x.StructuredFolderName.Equals(Properties.Settings.Default.projectfolder)) == null)
                return;

            var folderName = Properties.Settings.Default.projectfolder.Replace("\\Blocks", "\\Symbols");
            var folders = prj.AllFolders.ToArray();
            var folder = folders.FirstOrDefault(x => x.StructuredFolderName == folderName) as DotNetSiemensPLCToolBoxLibrary.DataTypes.Projectfolders.Step7V5.SymbolTable;

            if (folder == null || folder.SymbolTableEntrys == null)
                return;

            Symbols = folder.SymbolTableEntrys;

            tables = new Dictionary<string, DeviceCollection>();

            GetTable("NN", "(_NN_)");
            GetTable("VN", "(_VN_)");
            GetTable("UPS", "(_EDG_)");
            GetTable("VZT", "(_VZT_)");
            GetTable("OSV", "(_OSV_)");
            GetTable("POZ", "(_POZ_)");
            GetTable("MFV", "(_MFV_)");
            GetTable("EZS", "(_EZS_)");

            GetCustomTables();
        }

        public static string Location
        {
            get
            {
                return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(DefinitionCollection)).Location) + @"\definitions";
            }
        }

        private void GetCustomTables()
        {
            if (!Directory.Exists(Location))
                return;

            try
            {
                var files = Directory.GetFiles(Location, "*.jsond");

                foreach (var f in files)
                {
                    var d = DeviceCollection.Deserialize(File.ReadAllText(f));
                    if (d.Name == null)
                        d.Name = Path.GetFileNameWithoutExtension(f);

                    if (!tables.ContainsKey(d.Name))
                        tables.Add(d.Name, d);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"GetCustomTables error: {ex.Message}");
            }
        }

        private DeviceCollection GetTable(string name, string pattern)
        {
            var devices = new DeviceCollection();
            tables.Add(name, devices);

            var signals = Symbols.Where(x => Regex.IsMatch(x.Symbol, pattern));

            //var x1 = signals.First().Symbol.Substring(0, signals.First().Symbol.IndexOf('_'));
            ///var x2 = GetDevice(signals.First().Symbol, name);

            var devicesOrdered = signals.GroupBy(x => new { location = x.Symbol.Substring(0, x.Symbol.IndexOf('_')), device = GetDevice(x.Symbol, name) }).Select(g => new { key = g.Key, value = g.First() }).OrderBy(o => o.key.device);

            devices.AddRange(devicesOrdered.Select(x => new Device()
            {
                DeviceSymbol = (x.value.Symbol.EndsWith("_OUT_Of") || x.value.Symbol.EndsWith("_OUT_On")) ? x.value.Symbol.Substring(0, x.value.Symbol.LastIndexOf("_OUT_")) : x.value.Symbol.Substring(0, x.value.Symbol.LastIndexOf('_')),
                Point =       ((x.value.Symbol.EndsWith("_OUT_Of") || x.value.Symbol.EndsWith("_OUT_On")) ? x.value.Symbol.Substring(0, x.value.Symbol.LastIndexOf("_OUT_")) : x.value.Symbol.Substring(0, x.value.Symbol.LastIndexOf('_'))).Replace("_", "."),
                Comment = x.value.Comment,
                Description = GetDescription(x.key.device, x.value.Comment),
                Tooltip = GetToolTip(x.key.device, x.value.Comment),
                AlarmDescription = GetAlarmDescription(x.key.device, x.value.Comment),
                Location = x.value.Symbol.Substring(0,x.value.Symbol.IndexOf("_"))
            }));


            return devices;
        }

        string GetDescription(string device, string comment)
        {
            int i;
            string description;

            if ((i = device.IndexOf("_")) > 0)
                description = device.Substring(i + 1);
            else
                description = device;

            if (Regex.IsMatch(comment, "(meranie)"))
            {
                if (Regex.IsMatch(description, @"^IL\d$"))
                    description = $"I{description.Substring(2)}";
                else if (Regex.IsMatch(description, @"^UL\dN$"))
                    description = $"U{description.Substring(2, 1)}";
                else if (Regex.IsMatch(description, @"^UL\dL\d$"))
                    description = $"U{description.Substring(2, 1)}{description.Substring(4, 1)}";
            }

            return description;
        }
        string GetAlarmDescription(string device, string comment)
        {
            string message;
            int i = comment.LastIndexOf(" - ");
            if (i > 0)
                message = comment.Substring(0, i);
            else
                message = comment;

            if (message.StartsWith("NN") || message.StartsWith("VN"))
            {
                if ((i = device.IndexOf("_")) > 0)
                    message = message.Insert(3, device.Substring(0,i) + " ");
            }

            return textCorrection(message);
        }

        string GetToolTip(string device, string comment)
        {
            if (Regex.IsMatch(comment, "(meranie)"))
                return "";


            string tooltip;

            if (Regex.IsMatch(comment, "(Zasuvky pre hasicsku techniku)"))
            {
                tooltip = comment.Substring(0, comment.LastIndexOf(" ") - 1);
            }
            else
            {
                int i = comment.LastIndexOf(" - ");
                if (i > 0)
                    tooltip = comment.Substring(0, i - 1);
                else
                    tooltip = comment;
            }

            if (tooltip.StartsWith("NN ") || tooltip.StartsWith("VN "))
                tooltip = tooltip.Substring(3);

            #region uprava diakritiky
            tooltip = textCorrection(tooltip);

            #endregion

            return tooltip;
        }

        private static string textCorrection(string tooltip)
        {
            tooltip = tooltip.Replace("Odpojovac", "Odpojovač");
            tooltip = tooltip.Replace("Vypinac", "Vypínač");
            tooltip = tooltip.Replace("Uzemnovac", "Uzemňovač");
            tooltip = tooltip.Replace("Odpinac", "Odpínač");


            tooltip = tooltip.Replace("privod", "prívod");
            tooltip = tooltip.Replace("Privod", "Prívod");
            tooltip = tooltip.Replace("vyvod", "vývod");
            tooltip = tooltip.Replace("Vyvod", "Vývod");


            tooltip = tooltip.Replace("Napajanie", "Napájanie");
            tooltip = tooltip.Replace("zasuvky", "zásuvky");
            tooltip = tooltip.Replace("Zasuvkove", "Zásuvkové");
            tooltip = tooltip.Replace("Ostatne", "Ostatné");
            tooltip = tooltip.Replace("nezaloh", "nezáloh");
            tooltip = tooltip.Replace("nadrz", "nádrž");
            tooltip = tooltip.Replace("Poziar", "Požiar");
            tooltip = tooltip.Replace("sachta", "šachta");
            tooltip = tooltip.Replace("kanalizacie", "kanalizácie");
            tooltip = tooltip.Replace("Rozvadzac", "Rozvádzač");
            tooltip = tooltip.Replace("Vlastna", "Vlastná");
            tooltip = tooltip.Replace("Tunelovy", "Tunelový");
            tooltip = tooltip.Replace("Technologicky", "Technologický");
            tooltip = tooltip.Replace("ventilator", "ventilátor");
            tooltip = tooltip.Replace("Havarijna", "Havarijná");
            tooltip = tooltip.Replace("Stavidlova", "Stavidlová");
            tooltip = tooltip.Replace("Cerpacia", "čerpacia");
            tooltip = tooltip.Replace("Rozvodna", "Rozvodňa");
            tooltip = tooltip.Replace("Radiove", "Rádiové");
            tooltip = tooltip.Replace("napatie", "napätie");
            tooltip = tooltip.Replace("zapad", "západ");
            tooltip = tooltip.Replace("vychod", "východ");
            tooltip = tooltip.Replace("spinac", "spínač");
            tooltip = tooltip.Replace("Zavora", "Závora");
            tooltip = tooltip.Replace("Napajaci", "Napájací");
            tooltip = tooltip.Replace("Informacny", "Informačný");
            tooltip = tooltip.Replace("system", "systém");
            tooltip = tooltip.Replace("dialnice", "diaľnice");
            tooltip = tooltip.Replace("ovladacie", "ovládacie");
            tooltip = tooltip.Replace("hlavny", "hlavný");
            tooltip = tooltip.Replace("pozdÃzna", "pozdľžná");
            tooltip = tooltip.Replace("rychlosti", "rýchlosti");
            tooltip = tooltip.Replace("Mobilny", "Mobilný");
            tooltip = tooltip.Replace("Otvaracie", "Otváracie");
            tooltip = tooltip.Replace("QA11 PP8 Vydod do AN37P- stav Vypnuty", "QA11 PP8 Vývod do AN37P");
            tooltip = tooltip.Replace("hasicsku", "hasičskú");
            tooltip = tooltip.Replace("Kabina", "Kabína");
            return tooltip;
        }

        string GetDevice(string symbol, string table)
        {
            var temp = symbol.Substring(symbol.IndexOf('_', 5) + 1);

            var i = temp.LastIndexOf("_OUT");
            if (i > 0)
                return temp.Substring(0, i);
            else
            {
                i = temp.LastIndexOf("_");
                if (i > 0)
                    return temp.Substring(0, i);
                else
                    return table;
            }
        }

        public List<SymbolTableEntry> Symbols
        {
            get; private set;
        }
    }
}
