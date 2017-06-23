using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolFrontend
{
    public class DbClass : IEquatable<DbClass>, IComparable<DbClass>
    {
        public DbClass()
        {

        }

        public int Number
        { get; set; }

        public string Name
        { get; set; }

        public int Items
        { get; set; }

        public string ItemDependencies
        { get; set; }

        public string Path
        { get; set; }

        public DbStructure Structure
        { get; set; }


        public override string ToString()
        {
            return $"DB{Number}({Name})  Položek:{Items}  Typy:{ItemDependencies}";
        }

        public bool Equals(DbClass other)
        {
            return Number == other.Number;
        }

        public int CompareTo(DbClass other)
        {
            return Equals(other) ? 0 : ((Number > other.Number) ? 1 : -1);
        }

        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented });
        }

        public static string Serialize(DbClass obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented });
        }

        public static DbClass Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<DbClass>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
        }
    }
}
