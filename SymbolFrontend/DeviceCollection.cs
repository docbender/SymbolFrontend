using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolFrontend
{
    [JsonArray]
    public class DeviceCollection : List<Device>
    {
        public string Name
        {
            get;set;
        }

        /// <summary>
        /// Indicate custom table definition (jsond file)
        /// </summary>
        public bool Custom { get; set; } = false;

        public static DeviceCollection Deserialize(string json)
        {
            var devices = JsonConvert.DeserializeObject<DeviceCollection>(json, new JsonSerializerSettings() { });
            devices.Custom = true;
            return devices;
        }
    }
}
