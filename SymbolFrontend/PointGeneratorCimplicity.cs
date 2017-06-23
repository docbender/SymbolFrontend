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

                    var d = definitions.Get(i);

                    if (d == null)
                    {
                        logger.Error($"Nenalezena zadna definice pointu pro radek {i.ToString()}");
                        continue;
                    }


                    foreach (var pd in d.Points)
                    {
                        var ps = pointStructures.Get(pd.PointType);

                        if (ps == null)
                        {
                            logger.Error($"Nenalezena zadna struktura pointu typu {pd.PointType} pro radek {i.ToString()}");
                            continue;
                        }

                        var dlKey = Regex.IsMatch(i.Name, "(EDG)") ? "UPS" : Regex.IsMatch(i.Name, "(VN)") ? "VN" : "NN";
                        var point = createPoint(db, i, pd, ps, deviceLists[dlKey]);
                        points.Add(point);

                        //pokud je to point urciteho typu umoznit pretypovat na jine zarizeni napr spinac se signaly trafa
                    }
                }

                ToCsv(points, Location + $@"\{db.Name}.txt");
            });
        }

        static CimplicityPointStructure createPoint(DbClass db, DbStructure dbRow, PointDefinition definition, 
            CimplicityPointStructure structure, DeviceCollection deviceList)
        {
            var s = structure.Clone();

            var prefix = dbRow.Comment.Substring(0, dbRow.Comment.IndexOf('-')).Trim().Replace(" ", ".");

            var deviceDescription = GetDescription(db, dbRow, prefix, deviceList);
            var datablock = $"DB{db.Number}";
            int baseAddress = dbRow.Address.ByteAddress;

            s.RESOURCE_ID = GetResource(db, dbRow, prefix);
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

                var address = definition.DeviceAddress != null && definition.DeviceAddress.Length > 0
                    ? (definition.PlcArea.Equals("DBX", StringComparison.InvariantCultureIgnoreCase)
                        ? $"{datablock}.{definition.PlcArea}{baseAddress + definition.PlcAddressByte}.{definition.PlcAddressBit}"
                        : (definition.PlcArea.Equals("DBDF", StringComparison.InvariantCultureIgnoreCase)
                            ? $"{datablock}.DBD{baseAddress + definition.PlcAddressByte}:REAL"
                            : $"{datablock}.{definition.PlcArea}{baseAddress + definition.PlcAddressByte}"))
                    : datablock + definition.DeviceAddress;

                if (definition.OpcGroup != null && definition.OpcGroup.Length > 0)
                    s.ADDR = $"$[{definition.OpcGroup}]ns=2;s=TPCH.PLC.{address}";
                else
                    s.ADDR = $"ns=2;s=TPCH.PLC.{address}";
            }

            if (definition.AlarmEnabled)
            {
                s.ALM_ENABLE = "1";
                s.ALM_CLASS = definition.AlarmClass;
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

        static string GetResource(DbClass db, DbStructure dbRow, string device)
        {
            if (Regex.IsMatch(dbRow.Comment, "(NN)|(VN)|(EDG)"))
                return "ENERGIA";
            else
                return "OSTATNE";
        }

        static string GetDescription(DbClass db, DbStructure dbRow, string device, DeviceCollection deviceList)
        {
            var symbol = deviceList.FirstOrDefault(x => x.Point.Equals(device, StringComparison.InvariantCultureIgnoreCase));

            if(symbol==null)
                logger.Error($"Nenalezena zadna definice pointu {device} v seznamu zarizeni");

            return symbol?.Tooltip;
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

            File.WriteAllLines(file, lines);
        }
    }
}
