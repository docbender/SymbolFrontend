using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolFrontend
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PointDeviceDefinition
    {
        [JsonConstructor]
        public PointDeviceDefinition()
        {
            Points = new List<PointDefinition>();
        }

        [JsonProperty]
        public string Name
        { get; set; }

        [JsonProperty]
        public string TypeRestriction
        { get; set; }

        [JsonProperty]
        public string DeviceRestriction
        { get; set; }

        [JsonProperty]
        public string CommentRestriction
        { get; set; }

        [JsonProperty]
        public string DeviceRename
        { get; set; }        

        [JsonProperty]
        public List<PointDefinition> Points
        { get; set; }

        public override string ToString()
        {
            return $"{Name}:  Typ={TypeRestriction??"Vše"} Komentář={CommentRestriction ?? ""} Zařízení={DeviceRestriction ?? ""}";
        }

        public static PointDeviceDefinition Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<PointDeviceDefinition>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
        }
    }
}
