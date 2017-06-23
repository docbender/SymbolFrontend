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
    public class PointDeviceDefinitionTests
    {
        [TestMethod()]
        public void FillPointDefinitionFile()
        {
            var p01 = new PointDefinition();
            p01.AlarmClass = "AE";
            p01.AlarmDelay = 5;
            p01.AlarmEnabled = true;
            p01.AlarmLimit = 1;
            p01.AlarmMessage = "Alarm";
            p01.DataType = "BOOL";
            p01.Description = "point";
            p01.DeviceAddress = "M0.1";
            p01.Equation = "";
            p01.OpcGroup = "[GROUP01]";
            p01.PlcAddressBit = 1;
            p01.PlcAddressByte = 0;
            p01.PlcArea = "M";
            p01.PointType = "device";
            p01.PoolAfterSet = false;
            p01.ReadOnly = true;
            p01.Sufix = ".TEST";

            var p02 = new PointDefinition();
            p02.AlarmClass = "AE";
            p02.AlarmDelay = 5;
            p02.AlarmEnabled = true;
            p02.AlarmLimit = 1;
            p02.AlarmMessage = "Alarm";
            p02.DataType = "BOOL";
            p02.Description = "point";
            p02.DeviceAddress = "M0.1";
            p02.Equation = "";
            p02.OpcGroup = "[GROUp02]";
            p02.PlcAddressBit = 1;
            p02.PlcAddressByte = 0;
            p02.PlcArea = "M";
            p02.PointType = "device";
            p02.PoolAfterSet = false;
            p02.ReadOnly = true;
            p02.Sufix = ".TEST";

            var d = new PointDeviceDefinition() { Name = "device", TypeRestriction="UDT930", CommentRestriction="(spinac)", DeviceRestriction="^NN" };
            d.Points.Add(p01);
            d.Points.Add(p02);

            var s = JsonConvert.SerializeObject(d, new JsonSerializerSettings() { Formatting = Formatting.Indented });

            Assert.IsNotNull(s);
            Assert.IsTrue(s.Length > 0);

            System.IO.File.WriteAllText(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetAssembly(typeof(PointDeviceDefinitionTests)).Location) + @"\pointdevicedefinition.json", s);

        }

        [TestMethod()]
        public void DeserializeTest()
        {
            Assert.Fail();
        }
    }
}