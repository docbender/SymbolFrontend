using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolFrontend
{
    [JsonDictionary]
    class C32n : Dictionary<string, CimplicityPointStructure>
    {
        public static string Location
        {
            get
            {
                return System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(DefinitionCollection)).Location) + @"\definitions";
            }
        }

        public void Load()
        {
            this.Clear();

            if (!Directory.Exists(Location))
                return;

            try
            {
                var files = Directory.GetFiles(Location, "*.jsonp");

                foreach (var f in files)
                {
                    var d = CimplicityPointStructure.Deserialize(File.ReadAllText(f));
                    var name = Path.GetFileNameWithoutExtension(f).ToLower();

                    if (!this.ContainsKey(name))
                        this.Add(name, d);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public CimplicityPointStructure Get(string pointType)
        {
            return this[pointType.ToLower()];
        }
    }
}
