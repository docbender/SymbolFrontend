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

        [JsonProperty]
        public AddressClass Address { get; protected set; }

        [JsonProperty]
        public override List<IDataRow> Children { get; protected set; }

        // [JsonProperty]
        //public override int ByteLength { get; protected set; }

        [JsonProperty]
        public override string Name { get; set; }

        [JsonProperty]
        public override string Comment { get; set; }

        [JsonProperty]
        public string DataTypeField { get; set; }

        public override string DataTypeAsString { get { return DataTypeField; } }
    }
    }
