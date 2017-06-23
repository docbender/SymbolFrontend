using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using SymbolFrontend;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolFrontend.Test
{
    [TestClass()]
    public class PointDefinitionTests
    {
        [TestMethod()]
        public void FillPointDefinitionFile()
        {
            var p = new PointDefinition();
            p.AlarmClass = "AE";
            p.AlarmDelay = 5;
            p.AlarmEnabled = true;
            p.AlarmLimit = 1;
            p.AlarmMessage = "Alarm";
            p.DataType = "BOOL";
            p.Description = "point";
            p.DeviceAddress = "M0.1";
            p.Equation = "";
            p.OpcGroup = "[GROUP01]";
            p.PlcAddressBit = 1;
            p.PlcAddressByte = 0;
            p.PlcArea = "M";
            p.PointType = "device";
            p.PoolAfterSet = false;
            p.ReadOnly = true;
            p.Sufix = ".TEST";

            var s = JsonConvert.SerializeObject(p, new JsonSerializerSettings() { Formatting = Formatting.Indented });

            Assert.IsNotNull(s);
            Assert.IsTrue(s.Length > 0);

            System.IO.File.WriteAllText(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(PointDefinitionTests)).Location) + @"\pointdefinition.json", s);

        }

        [TestMethod()]
        public void DeserializeTest()
        {
            Assert.Fail();
        }
    }
}