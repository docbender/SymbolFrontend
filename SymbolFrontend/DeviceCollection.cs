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

        public static DeviceCollection Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<DeviceCollection>(json, new JsonSerializerSettings() { });
        }
    }
}
