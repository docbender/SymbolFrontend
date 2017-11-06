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

        /// <summary>
        /// Definition name
        /// </summary>
        [JsonProperty]
        public string Name
        { get; set; }

        /// <summary>
        /// Simatic datablock row datatype
        /// </summary>
        /// <example>UDTxx, BOOL, WORD,...</example>
        [JsonProperty]
        public string TypeRestriction
        { get; set; }

        /// <summary>
        /// Device name restriction (regex)
        /// </summary>
        /// <example>"_Rdy$" - only name ended with "_Rdy"</example>
        [JsonProperty]
        public string DeviceRestriction
        { get; set; }

        /// <summary>
        /// Enable remove name part matched by restriction regex
        /// </summary>
        [JsonProperty]
        public bool RemoveDeviceRestrictionFromDeviceName
        { get; set; } = false;

        /// <summary>
        /// Use symbol as device name (normally comment is used)
        /// </summary>
        [JsonProperty]
        public bool UseSymbolAsDeviceName
        { get; set; } = false;

        /// <summary>
        /// Comment name restriction (regex)
        /// </summary>
        [JsonProperty]
        public string CommentRestriction
        { get; set; }

        /// <summary>
        /// Key|Value pairs (delimited by semicolon) used to rename device name. Rename uses regex.
        /// </summary>
        /// <example>"1AN11.1QA|1T11;1AN12.1QA|1T12"</example>
        [JsonProperty]
        public string DeviceRename
        { get; set; }

        /// <summary>
        /// Key|Value pairs (delimited by semicolon) used to find aliased device name (Regex).
        /// </summary>
        /// <example>"1AN11.1QA|1T11;1AN12.1QA|1T12"</example>
        [JsonProperty]
        public string DeviceAlias
        { get; set; }

        /// <summary>
        /// Device Cimplicity resource
        /// </summary>
        [JsonProperty]
        public string Resource
        { get; set; } = "OSTATNE";

        /// <summary>
        /// Device point definitions list
        /// </summary>
        [JsonProperty]
        public List<PointDefinition> Points
        { get; set; }

        /// <summary>
        /// Is this virtul device(not in PLC)
        /// </summary>
        [JsonProperty]
        public bool Virtual
        { get; set; } = false;

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
