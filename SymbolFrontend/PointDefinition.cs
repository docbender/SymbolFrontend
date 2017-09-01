using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolFrontend
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PointDefinition
    {
        public static PointDefinition Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<PointDefinition>(json, new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Auto });
        }

        /// <summary>
        /// Typ pointu device, virtual, ...
        /// </summary>
        [JsonProperty]
        public string PointType
        { get; set; }

        /// <summary>
        /// Sufix pointu pro zarizeni
        /// </summary>
        [JsonProperty]
        public string Sufix
        { get; set; }

        /// <summary>
        /// Datovy typ Cimplicity
        /// </summary>
        [JsonProperty]
        public string DataType
        { get; set; }

        /// <summary>
        /// Rovnice pro vypocet virtualnich pointu.
        /// Pokud obsahuje @ jedna se o zastupny znak prefixu zarizeni
        /// </summary>
        [JsonProperty]
        public string Equation
        { get; set; }

        [JsonProperty]
        public string Description
        { get; set; }

        [JsonProperty]
        public bool AlarmEnabled
        { get; set; } = false;

        [JsonProperty]
        public string AlarmMessage
        { get; set; }

        [JsonProperty]
        public int AlarmLimit
        { get; set; } = 1;

        [JsonProperty]
        public string AlarmClass
        { get; set; } = "E";

        [JsonProperty]
        public int AlarmDelay
        { get; set; } = 0;

        [JsonProperty]
        public bool ReadOnly
        { get; set; } = false;

        [JsonProperty]
        public bool PoolAfterSet
        { get; set; } = false;

        /// <summary>
        /// Pokud prazdne sklada se addresa z PlcArea,PlcAddressByte,PlcAddressBit.
        /// Jinak z DeviceAddress a ostatni se ignoruje
        /// </summary>
        [JsonProperty]        
        public string DeviceAddress
        { get; set; }

        [JsonProperty]
        public string PlcArea
        { get; set; }

        [JsonProperty]
        public int PlcAddressByte
        { get; set; }

        [JsonProperty]
        public int PlcAddressBit
        { get; set; }

        [JsonProperty]
        public string OpcGroup
        { get; set; }

        [JsonProperty]
        public double Deadband
        { get; set; } = 0.0;

        [JsonProperty]
        public double Conversion
        { get; set; } = 0.0;
    }
}
