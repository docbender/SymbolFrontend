using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SymbolFrontend
{
    class PointGeneratorCimplicity
    {
        private static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string Location
        {
            get
            {
                return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(access)).Location) + @"\points";
            }
        }

        public static async Task Run(DbCollection dbc, DefinitionCollection definitions, C32n pointStructures, Dictionary<string, DeviceCollection> deviceLists)
        {
            if (!Directory.Exists(Location))
            {
                try
                {
                    Directory.CreateDirectory(Location);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            var tasks = new List<Task>();

            foreach (var db in dbc)
            {
                tasks.Add(Generate(db, definitions, pointStructures, deviceLists));
            }

            await Task.WhenAll(tasks.ToArray());
        }

        static Task Generate(DbClass db, DefinitionCollection definitions, C32n pointStructures, Dictionary<string, DeviceCollection> deviceLists)
        {
            return Task.Run(() =>
            {
                List<CimplicityPointStructure> points = new List<CimplicityPointStructure>();

                foreach (DbStructure i in db.Structure.Children)
                {

                    var dl = definitions.Get(i);

                    if (dl == null)
                    {
                        logger.Error($"Nenalezena zadna definice pointu pro radek {i.ToString()}");
                        continue;
                    }

                    foreach (var d in dl)
                    {
                        foreach (var pd in d.Points)
                        {
                            var ps = pointStructures.Get(pd.PointType);

                            if (ps == null)
                            {
                                logger.Error($"Nenalezena zadna struktura pointu typu {pd.PointType} pro radek {i.ToString()}");
                                continue;
                            }

                            var point = createPoint(db, i, pd, d, ps, deviceLists);
                            if (point != null)
                                points.Add(point);
                        }
                    }
                }

                ToCsv(points, Location + $@"\{db.Name}.txt");
            });
        }

        static CimplicityPointStructure createPoint(DbClass db, DbStructure dbRow, PointDefinition definition,
            PointDeviceDefinition deviceDefinition, CimplicityPointStructure structure, Dictionary<string, DeviceCollection> deviceLists)
        {
            var s = structure.Clone();
            string prefix;
            if (deviceDefinition.UseSymbolAsDeviceName)
            {
                if (deviceDefinition.DeviceRestriction != null && deviceDefinition.DeviceRestriction.Length > 0 && deviceDefinition.RemoveDeviceRestrictionFromDeviceName)
                    prefix = Regex.Replace(dbRow.Name, deviceDefinition.DeviceRestriction, "").Replace("_", ".");
                else
                    prefix = dbRow.Name.Replace("_", ".");
            }
            else
                prefix = dbRow.Comment.Substring(0, dbRow.Comment.IndexOf('-')).Trim().Replace(" ", ".");

            if (deviceDefinition.DeviceRename != null)
            {
                var pairs = deviceDefinition.DeviceRename.Split(';');

                foreach (var str in pairs)
                {
                    var pair = str.Split('|');

                    if (pair.Length > 1)
                    {
                        if (prefix.Contains(pair[0]))
                        {
                            prefix = prefix.Replace(pair[0], pair[1]);
                            break;
                        }
                    }
                }
            }

            var dlKey = Regex.IsMatch(dbRow.Comment, "(NN)") ? "NN" :
                Regex.IsMatch(dbRow.Comment, "(VN)") ? "VN" :
                Regex.IsMatch(dbRow.Comment, "(EDG)") ? "UPS" : "";
            Device deviceSymbol = null;
            if (dlKey.Length > 0)
                deviceSymbol = deviceLists[dlKey].FirstOrDefault(y => y.Point.Equals(prefix, StringComparison.InvariantCultureIgnoreCase));
            if (deviceSymbol == null)
                deviceSymbol = deviceLists.SelectMany(x => x.Value.Where(y => y.Point.Equals(prefix, StringComparison.InvariantCultureIgnoreCase))).FirstOrDefault();
            if (deviceSymbol == null)
            {
                logger.Error($"Nenalezeno zarizeni pro point {prefix} z DB{db.Number}.{dbRow.Address}");
                return null;
            }

            var datablock = $"DB{db.Number}";


            s.RESOURCE_ID = deviceDefinition.Resource;
            s.PT_ID = $"{prefix}{definition.Sufix}";
            s.DESC = definition.Description;
            s.PT_TYPE = definition.DataType;
            s.ACCESS = definition.ReadOnly ? "R" : "W";

            if (definition.PointType.Equals("virtual", StringComparison.InvariantCultureIgnoreCase))
                s.EQUATION = definition.Equation.Replace("@", prefix);

            if (definition.PointType.Equals("device", StringComparison.InvariantCultureIgnoreCase))
            {
                s.DEVICE_ID = "PLC";
                s.UPDATE_CRITERIA = "UC";
                s.POLL_AFTER_SET = definition.PoolAfterSet ? "1" : "0";

                string address;
                if (definition.DeviceAddress != null && definition.DeviceAddress.Length > 0)
                    address = datablock + definition.DeviceAddress;
                else
                {
                    int baseAddress = dbRow.Address.ByteAddress * 8 + dbRow.Address.BitAddress;
                    int itemAddress = baseAddress + definition.PlcAddressByte * 8 + definition.PlcAddressBit;

                    address = (definition.PlcArea.Equals("DBX", StringComparison.InvariantCultureIgnoreCase)
                            ? $"{datablock}.DBX{itemAddress / 8}.{itemAddress % 8}"
                            : (definition.PlcArea.Equals("DBDF", StringComparison.InvariantCultureIgnoreCase)
                                ? $"{datablock}.DBD{itemAddress / 8}:REAL"
                                : $"{datablock}.{definition.PlcArea}{itemAddress / 8}"));
                }

                if (definition.OpcGroup != null && definition.OpcGroup.Length > 0)
                    s.ADDR = $"$[{definition.OpcGroup}]ns=2;s=TPCH.PLC.{address}";
                else
                    s.ADDR = $"ns=2;s=TPCH.PLC.{address}";
            }

            if (definition.AlarmEnabled)
            {
                var deviceDescription = GetDeviceAlarmDescription(db, dbRow, prefix, deviceSymbol);

                s.ALM_ENABLE = "1";
                s.ALM_CLASS = GetAlarmClass(definition.AlarmClass, deviceSymbol); //;
                s.ALM_HIGH_2 = definition.AlarmLimit.ToString();
                s.ALM_MSG = deviceDescription + " - " + definition.AlarmMessage;

                if (definition.AlarmClass.Equals("E"))
                {
                    s.ACK_TIMEOUT_HI = "0";
                    s.ACK_TIMEOUT_HIHI = "0";
                    s.ACK_TIMEOUT_LO = "0";
                    s.ACK_TIMEOUT_LOLO = "0";
                }

                if (!definition.DataType.Equals("BOOL", StringComparison.InvariantCultureIgnoreCase))
                    s.ALM_UPDATE_VALUE = 1;
            }

            if (definition.AlarmDelay > 0)
            {
                s.ALM_DELAY = 1;
                s.ALARM_DELAY_HI = definition.AlarmDelay;
                s.ALARM_DELAY_HIHI = definition.AlarmDelay;
                s.ALARM_DELAY_LO = definition.AlarmDelay;
                s.ALARM_DELAY_LOLO = definition.AlarmDelay;
                s.ALARM_DELAY_UNIT_HI = "SEC";
                s.ALARM_DELAY_UNIT_HIHI = "SEC";
                s.ALARM_DELAY_UNIT_LO = "SEC";
                s.ALARM_DELAY_UNIT_LOLO = "SEC";
            }


            if (definition.Deadband > 0)
                s.ANALOG_DEADBAND = definition.Deadband.ToString();

            return s;
        }

        static string GetAlarmClass(string sourceClass, Device symbol)
        {
            if (!sourceClass.Equals("E") && Regex.IsMatch(symbol.DeviceSymbol, "(NN)|(VN)|(EDG)"))
            {
                if (symbol.Location != null && symbol.Location.Equals("PTO1") || symbol.Location.Equals("PTO2"))
                    return $"{sourceClass}EP";
                else
                    return $"{sourceClass}ET";
            }
            else
                return sourceClass;
        }

        static string GetDeviceAlarmDescription(DbClass db, DbStructure dbRow, string device, Device symbol)
        {
            //var symbol = deviceList.FirstOrDefault(x => x.Point.Equals(device, StringComparison.InvariantCultureIgnoreCase));

            if (symbol == null)
                logger.Error($"Nenalezena zadna definice pointu {device}-DB{db.Number}.{dbRow.Address} v seznamu zarizeni");

            return symbol?.AlarmDescription;
        }

        static void ToCsv<T>(IEnumerable<T> list, string file)
        {
            FieldInfo[] fields = typeof(T).GetFields();
            PropertyInfo[] properties = typeof(T).GetProperties();


            var lines = new List<string>();
            lines.Add(string.Join(",", fields.Select(f => f.Name).Concat(properties.Select(p => p.Name)).ToArray()));

            foreach (var o in list)
            {
                lines.Add(string.Join(",", fields.Select(f => (f.GetValue(o) ?? "").ToString())
                    .Concat(properties.Select(p => (p.GetValue(o, null) ?? "").ToString())).ToArray()));
            }

            File.WriteAllLines(file, lines, Encoding.GetEncoding(1250));
        }
    }
}
