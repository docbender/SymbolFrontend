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

        /// <summary>
        /// Datablock number
        /// </summary>
        public int Number
        { get; set; }

        /// <summary>
        /// Datablock name
        /// </summary>
        public string Name
        { get; set; }

        /// <summary>
        /// Items count in datablock
        /// </summary>
        public int Items
        { get; set; }

        /// <summary>
        /// Dependencies (used types)
        /// </summary>
        public string ItemDependencies
        { get; set; }

        /// <summary>
        /// Path in project structure
        /// </summary>
        public string Path
        { get; set; }

        /// <summary>
        /// Datablock structure
        /// </summary>
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

        /// <summary>
        /// Serialize into JSON
        /// </summary>
        /// <returns></returns>
        public string Serialize()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented });
        }

        /// <summary>
        /// Serialize into JSON
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string Serialize(DbClass obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto, Formatting = Formatting.Indented });
        }

        /// <summary>
        /// Deserialize JSON string
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static DbClass Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<DbClass>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
        }
    }
}
