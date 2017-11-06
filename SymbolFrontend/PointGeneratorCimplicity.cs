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

            tasks.Add(GenerateVirtual(definitions, pointStructures, deviceLists));

            await Task.WhenAll(tasks.ToArray());
        }


        static Task GenerateVirtual(DefinitionCollection definitions, C32n pointStructures, Dictionary<string, DeviceCollection> deviceLists)
        {
            return Task.Run(() =>
            {
                var virt = definitions.Where(z => z.Value.Virtual);
                if (virt.Count() == 0)
                    return;

                log4net.ThreadContext.Properties["datablock"] = "Virtual";
                List<CimplicityPointStructure> points = new List<CimplicityPointStructure>();

                foreach (var d in virt)
                {
                    var device = deviceLists.SelectMany(x => x.Value.Where(z => z.VirtualType?.Equals(d.Value.Name) ?? false));

                    if (device.Count() == 0)
                    {
                        logger.Warn($"Nenalezeno zadne zarizeni k definici {d.Value.Name}");
                        continue;
                    }

                    foreach (var singledevice in device)
                    {
                        foreach (var pd in d.Value.Points)
                        {
                            var ps = pointStructures.Get(pd.PointType);

                            if (ps == null)
                            {
                                logger.Error($"Nenalezena zadna struktura pointu typu {pd.PointType}. Definice={d.Value.Name}");
                                continue;
                            }
                            points.Add(createPoint(pd, d.Value, ps, singledevice));
                        }
                    }
                }

                ToCsv(points, Location + $@"\virtual.txt");
            });
        }

        static Task Generate(DbClass db, DefinitionCollection definitions, C32n pointStructures, Dictionary<string, DeviceCollection> deviceLists)
        {
            return Task.Run(() =>
            {
                log4net.ThreadContext.Properties["datablock"] = db.Name;
                List<CimplicityPointStructure> points = new List<CimplicityPointStructure>();

                foreach (DbStructure i in db.Structure.Children)
                {

                    var dl = definitions.Get(i);

                    if (dl == null || dl.Count == 0)
                    {
                        logger.Error($"Nenalezena zadna definice pointu pro DB{db.Number}.{i.Address}. Symbol={i.Name}, Comment={i.Comment}");
                        continue;
                    }

                    foreach (var d in dl)
                    {
                        foreach (var pd in d.Points)
                        {
                            var ps = pointStructures.Get(pd.PointType);

                            if (ps == null)
                            {
                                logger.Error($"Nenalezena zadna struktura pointu typu {pd.PointType} pro DB{db.Number}.{i.Address}. Symbol={i.Name}, Comment={i.Comment}. Definice={d.Name}");
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

        static CimplicityPointStructure createPoint(PointDefinition definition,
            PointDeviceDefinition deviceDefinition, CimplicityPointStructure structure, Device device)
        {
            return createPoint(null, null, definition, deviceDefinition, structure, null, device);
        }

        static CimplicityPointStructure createPoint(DbClass db, DbStructure dbRow, PointDefinition definition,
            PointDeviceDefinition deviceDefinition, CimplicityPointStructure structure, Dictionary<string, DeviceCollection> deviceLists)
        {
            return createPoint(db, dbRow, definition, deviceDefinition, structure, deviceLists, null);
        }

        static CimplicityPointStructure createPoint(DbClass db, DbStructure dbRow, PointDefinition definition,
            PointDeviceDefinition deviceDefinition, CimplicityPointStructure structure, Dictionary<string, DeviceCollection> deviceLists,
            Device device)
        {
            if (dbRow == null && device == null)
                return null;

            var s = structure.Clone();
            string prefix;
            Device deviceSymbol = null;
            string datablock = "";

            if (dbRow != null)
            {
                if (deviceDefinition.UseSymbolAsDeviceName)
                {
                    if (deviceDefinition.DeviceRestriction != null && deviceDefinition.DeviceRestriction.Length > 0 && deviceDefinition.RemoveDeviceRestrictionFromDeviceName)
                        prefix = Regex.Replace(dbRow.Name, deviceDefinition.DeviceRestriction, "").Replace("_", ".");
                    else
                        prefix = dbRow.Name.Replace("_", ".");

                    if (db.Number == 910 || db.Number == 1010)
                    {
                        var dot = prefix.IndexOf('.');

                        if (!prefix.Substring(dot + 1).StartsWith("VN") && !prefix.Substring(dot + 1).StartsWith("NN"))
                            prefix = prefix.Substring(0, dot) + ".NN" + prefix.Substring(dot);
                    }
                }
                else if (db.Number != 901 || !dbRow.Comment.Contains('-'))
                    prefix = dbRow.Name.Replace("_", ".");
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
                            if (Regex.IsMatch(prefix, pair[0]))
                            {
                                prefix = Regex.Replace(prefix, pair[0], pair[1]);
                                break;
                            }
                            /*if (prefix.Contains(pair[0]))
                            {
                                prefix = prefix.Replace(pair[0], pair[1]);
                                break;
                            }*/
                        }
                    }
                }

                string devicePrefixToFind = prefix;

                if (deviceDefinition.DeviceAlias != null)
                {
                    var pairs = deviceDefinition.DeviceAlias.Split(';');

                    foreach (var str in pairs)
                    {
                        var pair = str.Split('|');

                        if (pair.Length > 1)
                        {
                            if (Regex.IsMatch(devicePrefixToFind, pair[0]))
                            {
                                devicePrefixToFind = Regex.Replace(devicePrefixToFind, pair[0], pair[1]);
                                break;
                            }
                        }
                    }
                }

                var dlKey = Regex.IsMatch(dbRow.Comment, "(EDG)") ? "UPS" :
                    Regex.IsMatch(dbRow.Comment, "(NN)") ? "NN" :
                    Regex.IsMatch(dbRow.Comment, "(VN)") ? "VN" :
                     "";

                var custom = deviceLists.Where(x => x.Value.Custom).SelectMany(z => z.Value.Where(y => y.Point.Equals(devicePrefixToFind, StringComparison.InvariantCultureIgnoreCase))).FirstOrDefault();

                if (custom != null)
                    deviceSymbol = custom;
                else if (dlKey.Length > 0)
                    deviceSymbol = deviceLists[dlKey].FirstOrDefault(y => y.Point.Equals(devicePrefixToFind, StringComparison.InvariantCultureIgnoreCase));
                if (deviceSymbol == null)
                    deviceSymbol = deviceLists.SelectMany(x => x.Value.Where(y => y.Point.Equals(devicePrefixToFind, StringComparison.InvariantCultureIgnoreCase))).FirstOrDefault();
                if (deviceSymbol == null)
                {
                    logger.Error($"Nenalezeno zarizeni pro point {devicePrefixToFind} z DB{db.Number}.{dbRow.Address}. Symbol={dbRow.Name}, Comment={dbRow.Comment}, Definice={deviceDefinition.Name}");
                    return null;
                }

                datablock = $"DB{db.Number}";
            }
            else
            {
                prefix = device.Point;
                deviceSymbol = device;
            }


            s.RESOURCE_ID = deviceDefinition.Resource;
            s.PT_ID = $"{prefix}{definition.Sufix}";
            if (deviceDefinition.Name.Equals("jistic") || deviceDefinition.Name.Equals("pusobeni ochrany"))
            {
                var dsc = GetDeviceAlarmDescription(db, dbRow, prefix, deviceSymbol).Replace("NN ", "");
                dsc = dsc.Substring(dsc.IndexOf(" ") + 1);
                dsc = Regex.Replace(dsc, "( PP\\d+ )", " ");
                dsc = Regex.Replace(dsc, "( PTO\\d+ )", " ");
                s.DESC = "\"" + dsc + "\""; // dbRow.Comment.Substring(0,dbRow.Comment.LastIndexOf(" -"));
            }
            else
                s.DESC = definition.Description;
            s.PT_TYPE = definition.DataType;
            if (definition.Elements > 1)
                s.ELEMENTS = definition.Elements;
            s.ACCESS = definition.ReadOnly ? "R" : "W";

            if (definition.PointType.Equals("virtual", StringComparison.InvariantCultureIgnoreCase))
                s.EQUATION = definition.Equation.Replace("@", prefix);

            if (definition.EquationType != null)
                s.CALC_TYPE = definition.EquationType;

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
                                ? $"{datablock}.REAL{itemAddress / 8}"  //? $"{datablock}.DBD{itemAddress / 8}:REAL"
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

                if(s.ALM_MSG.Contains(','))
                    s.ALM_MSG = "\"" + s.ALM_MSG + "\"";

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
                s.ANALOG_DEADBAND = definition.Deadband.ToString("0.#####", System.Globalization.CultureInfo.InvariantCulture);

            if (definition.Conversion != 0)
            {
                s.CONV_TYPE = "CS";
                s.FW_CONV_EQ = $"%P * {definition.Conversion.ToString("0.#####", System.Globalization.CultureInfo.InvariantCulture)}";
                s.REV_CONV_EQ = $"%P / {definition.Conversion.ToString("0.#####", System.Globalization.CultureInfo.InvariantCulture)}";
            }

            if (definition.Trigger != null)
            {
                s.TRIG_PT = definition.Trigger;
            }

            if (definition.Reset != null)
            {
                s.RESET_PT = definition.Reset;
            }

            return s;
        }

        static string GetAlarmClass(string sourceClass, Device symbol)
        {
            if (sourceClass.Equals("E"))
                return sourceClass;

            if (Regex.IsMatch(symbol.Point, "(NN)|(VN)|(EDG)"))
            {
                if (symbol.Location != null && symbol.Location.Equals("PTO1") || symbol.Location.Equals("PTO2"))
                    return $"{sourceClass}EP";
                else
                    return $"{sourceClass}ET";
            }
            else if (Regex.IsMatch(symbol.Point, "(OSV)"))
                return $"{sourceClass}O";
            else if (Regex.IsMatch(symbol.Point, "(VZT)|(MFV)"))
                return $"{sourceClass}VO";
            else if (Regex.IsMatch(symbol.Point, "(POZ)"))
                return $"{sourceClass}VODA";
            else if (Regex.IsMatch(symbol.Point, "(EZS)"))
                return $"{sourceClass}BEZP";
            else if (Regex.IsMatch(symbol.Point, "(DRS)"))
                return $"{sourceClass}DRS";

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
