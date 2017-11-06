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

        /// <summary>
        /// Copy method
        /// </summary>
        /// <param name="db"></param>
        public void CopyFrom(DotNetSiemensPLCToolBoxLibrary.DataTypes.Blocks.Step7V5.S7DataBlock db)
        {
            Name = db.BlockName;
            Number = db.BlockNumber;
            Items = db.Structure.Children.Count;
            ItemDependencies = string.Join(";", db.Structure.Children.Select(x => (x as DotNetSiemensPLCToolBoxLibrary.DataTypes.Blocks.Step7V5.S7DataRow).DataTypeAsString).Distinct());
            Path = db.ParentFolder.StructuredFolderName;
            Structure = new SymbolFrontend.DbStructure(db.Structure);
        }

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
