using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolFrontend
{
    public class Device
    {
        public string Point
        {
            get; set;
        }

        public string DeviceSymbol
        {
            get; set;
        }

        public string Description
        {
            get; set;
        }

        public string AlarmDescription
        {
            get; set;
        }


        public string Tooltip
        {
            get; set;
        }

        public string Comment
        {
            get; set;
        }

        public override string ToString()
        {
            return $"Point={Point}";
        }
    }
}
