using DotNetSiemensPLCToolBoxLibrary.DataTypes.Blocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetSiemensPLCToolBoxLibrary.DataTypes;
using DotNetSiemensPLCToolBoxLibrary.DataTypes.Blocks.Step7V5;
using Newtonsoft.Json;

namespace SymbolFrontend
{

    [JsonObject(MemberSerialization.OptIn)]
    public class DbStructure : DataBlockRow
    {
        public DbStructure()
        {
        }
        public DbStructure(IDataRow structure)
        {
            this.Address = new AddressClass(structure.BlockAddress);

            Children = new List<IDataRow>();
            Name = structure.Name;
            Comment = structure.Comment;
            DataTypeField = (structure as DataBlockRow).DataTypeAsString;

            foreach (var i in structure.Children)
            {
                Children.Add(new DbStructure(i));
            }
        }

        /// <summary>
        /// Address
        /// </summary>
        [JsonProperty]
        public AddressClass Address { get; protected set; }

        /// <summary>
        /// Rows
        /// </summary>
        [JsonProperty]
        public override List<IDataRow> Children { get; protected set; }

        /// <summary>
        /// Name
        /// </summary>
        [JsonProperty]
        public override string Name { get; set; }

        /// <summary>
        /// Comment
        /// </summary>
        [JsonProperty]
        public override string Comment { get; set; }

        /// <summary>
        /// DataType
        /// </summary>
        [JsonProperty]
        public string DataTypeField { get; set; }

        /// <summary>
        /// Return type as string
        /// </summary>
        public override string DataTypeAsString { get { return DataTypeField; } }
    }
    }
