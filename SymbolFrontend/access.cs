using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolFrontend
{
    class access : ISymbolData
    {
        public string Location
        {
            get
            {
                if(Properties.Settings.Default.dbpath.Length == 0 || !File.Exists(Properties.Settings.Default.dbpath))
                    return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(access)).Location) + @"\db.accdb";

                return Properties.Settings.Default.dbpath;
            }
            set
            {
                Properties.Settings.Default.dbpath = value;
                Properties.Settings.Default.Save();
            }
        }
        public string Connection
        {
            get
            {
                return $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={Location};Persist Security Info=True";
            }
        }
        public List<string> GetTableList()
        {
            List<string> list = new List<string>();

            using (OleDbConnection db = new OleDbConnection(Connection))
            {
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = "SELECT * FROM MSysObjects WHERE Type=1 AND Flags=0";

                    try
                    {
                        db.Open();
                        var data = db.GetSchema("Tables");

                        //var reader = await cmd.ExecuteReaderAsync();

                        foreach (System.Data.DataRow r in data.Rows)
                            if(r[3].ToString() == "TABLE")
                                list.Add(r[2].ToString());
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }

            return list;
        }

        public async Task<DeviceCollection> GetTable(string table, string name)
        {
            DeviceCollection list = new DeviceCollection();

            list.Name = name;

            using (OleDbConnection db = new OleDbConnection(Connection))
            {
                using (var cmd = db.CreateCommand())
                {
                    cmd.CommandType = System.Data.CommandType.Text;
                    cmd.CommandText = $"SELECT [point], [Comment], [description], [tooltip], [DeviceSymbol], [Comment], [Location] FROM [{table}]";

                    try
                    {
                        db.Open();
                        var reader = await cmd.ExecuteReaderAsync();

                        while (reader.Read())
                        {
                            if(!reader.IsDBNull(0))
                            list.Add(new Device() {
                                Point = reader.GetString(0),
                                Comment = reader.IsDBNull(1) ? null : reader.GetString(1),
                                Description = reader.IsDBNull(2) ? null : reader.GetString(2),
                                AlarmDescription = reader.IsDBNull(5) ? null : reader.GetString(5),
                                DeviceSymbol = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Location = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Tooltip = reader.IsDBNull(3) ? null : reader.GetString(3) });
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        if (db.State == System.Data.ConnectionState.Open)
                            db.Close();
                    }
                }
            }

            return list;
        }

        public Dictionary<string, DeviceCollection> GetDeviceLists()
        {
            var tasks = new List<Task<DeviceCollection>>();
            tasks.Add(GetTable("ENN - seznam zarizeni", "NN"));
            tasks.Add(GetTable("EVN - seznam zarizeni", "VN"));
            tasks.Add(GetTable("EDG - seznam zarizeni", "UPS"));

            Task.WaitAll(tasks.ToArray());

            tasks[0].ContinueWith(t => { throw t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
            tasks[1].ContinueWith(t => { throw t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
            tasks[2].ContinueWith(t => { throw t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);

            var d = new Dictionary<string, DeviceCollection>();
            d.Add("NN", tasks[0].Result);
            d.Add("VN", tasks[1].Result);
            d.Add("UPS", tasks[2].Result);

            return d;
        }
    }
}
