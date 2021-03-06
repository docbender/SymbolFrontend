﻿using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SymbolFrontend
{
    public class DefinitionCollection : Dictionary<string, PointDeviceDefinition>
    {
        public static string Location
        {
            get
            {
                return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(DefinitionCollection)).Location) + @"\definitions";
            }
        }

        /// <summary>
        /// Get device definition according to datablock line
        /// </summary>
        /// <param name="row"></param>
        /// <returns></returns>
        public List<PointDeviceDefinition> Get(DbStructure row)
        {
            var defs = new List<PointDeviceDefinition>();

            foreach (var i in this.Values)
            {
                if (i.Virtual)
                    continue;

                bool can = false;

                if (i.TypeRestriction != null && i.TypeRestriction.Length > 0)
                {
                    if (!i.TypeRestriction.Equals(row.DataTypeAsString))
                        continue;
                    else
                        can = true;
                }

                if (i.DeviceRestriction != null && i.DeviceRestriction.Length > 0)
                {
                    if (!Regex.IsMatch(row.Name, i.DeviceRestriction))
                        continue;
                    else
                        can = true;
                }

                if (i.CommentRestriction != null && i.CommentRestriction.Length > 0)
                {
                    if (!Regex.IsMatch(row.Comment, i.CommentRestriction))
                        continue;
                    else
                        can = true;
                }

                if (can)
                {
                    defs.Add(i);
                }
            }


            return defs;
        }

        /// <summary>
        /// Get definition according to definition name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public PointDeviceDefinition Get(string name)
        {
            if (this.Keys.Contains(name))
                return this[name];
            else
                return null;
        }

        public Task Load()
        {
            this.Clear();

            Task t = Task.Run(() =>
            {
                if (!Directory.Exists(Location))
                    return;

                try
                {
                    var files = Directory.GetFiles(Location, "*.json");

                    foreach (var f in files)
                    {
                        var d = PointDeviceDefinition.Deserialize(File.ReadAllText(f));
                        if (d.Name == null)
                            d.Name = Path.GetFileNameWithoutExtension(f);

                        if (!this.ContainsKey(d.Name))
                            this.Add(d.Name, d);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            });

            return t;
        }
    }
}
