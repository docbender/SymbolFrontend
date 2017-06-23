using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SymbolFrontend
{
    class assigner
    {
        public static async Task Run(Dictionary<string, DeviceCollection> devicelists)
        {
            var filetasks = new List<Task>();
            filetasks.Add(AssignCtxFile(@"c:\TPCH\screens\Technologie\Energia\TPCH_Energia_PTO.ctx",
                new DeviceCollection[] { devicelists["NN"], devicelists["VN"], devicelists["UPS"] }));
            filetasks.Add(AssignCtxFile(@"c:\TPCH\screens\Technologie\Energia\TPCH_Energia_Tunel.ctx",
                new DeviceCollection[] { devicelists["NN"] }));

            await Task.WhenAll(filetasks);
        }

        static Task AssignCtxFile(string file, DeviceCollection[] deviceTables)
        {
            return Task.Run(() =>
            {
                try
                {
                    File.Copy(file, file + $".{DateTime.Now.ToString("yyMMdd-HHmmss")}");
                    string text = File.ReadAllText(file);

                    foreach (var t in deviceTables)
                    {
                        if (t.Name == "NN" || t.Name == "VN")
                            text = FindObjectAndFill(text, "E_PWB", t);

                        if (t.Name == "NN" || t.Name == "VN")
                            text = FindObjectAndFill(text, "E_TR", t);

                        if (t.Name == "UPS")
                            text = FindObjectAndFill(text, "E_UPS", t);

                        if (t.Name == "NN" || t.Name == "VN")
                            text = FindObjectAndFill(text, "AnalogValue", t);
                    }

                    File.WriteAllText(file, text, Encoding.GetEncoding(1200));
                }
                catch (Exception ex)
                {
                    throw new Exception($"{file}: {ex.Message}");
                }
            });
        }

        static string FindObjectAndFill(string fileContent, string objectName, DeviceCollection table)
        {
            int pos = 0, os;
            while ((pos = fileContent.IndexOf($"\"$object\" \"{objectName}\"", pos+1, StringComparison.InvariantCultureIgnoreCase)) > 0)
            {
                if ((os = fileContent.LastIndexOf("(GmmiOptionTable", pos, StringComparison.InvariantCultureIgnoreCase)) > 0)
                {
                    int brcnt = 1;
                    int cpos = os;
                    while (brcnt > 0)
                    {
                        cpos = cpos + 1;
                        char c = fileContent[cpos];
                        if (c == '(')
                            brcnt = brcnt + 1;
                        else if (c == ')')
                            brcnt = brcnt - 1;
                    }
                    int oe = cpos;

                    var newObject = ReplaceVariableValues(fileContent.Substring(os, oe - os + 1), table);
                    if (newObject != null)
                    {
                        fileContent = fileContent.Substring(0, os - 1) +
                            newObject +
                            fileContent.Substring(oe + 1);
                    }
                }
            }

            return fileContent;
        }

        static string ReplaceVariableValues(string objectString, DeviceCollection table)
        {
            int pos = objectString.IndexOf("\"point\"", StringComparison.InvariantCultureIgnoreCase);
            if (pos < 0)
                return null;

            int pns = objectString.IndexOf("\"", pos + 7);
            if (pns < 0)
                return null;

            int pne = objectString.IndexOf("\"", pns + 1);
            if (pne < 0)
                return null;


            var pointname = objectString.Substring(pns + 1, pne - pns - 1);

            var rec = table.FirstOrDefault(x => x.Point.Trim() == pointname);

            if (rec == null)
                return null;

            string newObjectString;
            if ((newObjectString = ReplaceVariableValue(objectString, "description", ((rec.Description == null || rec.Description.Length == 0) ? "" : "{&h22}" + rec.Description + "{&h22}"))) != null)
                objectString = newObjectString;

            if ((newObjectString = ReplaceVariableValue(objectString, "tooltiptext", ((rec.Tooltip == null || rec.Tooltip.Length == 0) ? "" : "{&h22}" + rec.Tooltip + "{&h22}"))) != null)
                objectString = newObjectString;


            string unit = null, chartTable = null, chartValueDescription = null, chartYTitle = null;

            if (Regex.IsMatch(pointname, @"(\.I)[^\.]*$")) /// pointname.EndsWith(".I"))
            {
                chartTable = "ENERGIA";
                chartValueDescription = rec.Comment;
                unit = "A";
                chartYTitle = $"I [{unit}]";
            }
            else if (Regex.IsMatch(pointname, @"(\.U)[^\.]*$")) ///(pointname.EndsWith(".U") || pointname.EndsWith(".U1"))
            {
                chartTable = "ENERGIA";
                chartValueDescription = rec.Comment;
                unit = pointname.Contains("VN") ? "kV" : "V";
                chartYTitle = $"U [{unit}]";
            }
            if (chartYTitle != null)
            {
                if ((newObjectString = ReplaceVariableValue(objectString, "chartTable", chartTable)) != null)
                    objectString = newObjectString;

                if ((newObjectString = ReplaceVariableValue(objectString, "chartValueDescription", chartValueDescription)) != null)
                    objectString = newObjectString;

                if ((newObjectString = ReplaceVariableValue(objectString, "chartYTitle", chartYTitle)) != null)
                    objectString = newObjectString;

                if ((newObjectString = ReplaceVariableValue(objectString, "unit", unit)) != null)
                    objectString = newObjectString;
            }

            return objectString;
        }

        static string ReplaceVariableValue(string objectString, string variableName, string variableValue)
        {
            if (variableValue == null)
                variableValue = "";

            int pos = objectString.IndexOf($"\"{variableName}\"", StringComparison.InvariantCultureIgnoreCase);
            if (pos <= 0)
                return null;

            int brs = objectString.LastIndexOf("(", pos);
            if (brs <= 0)
                return null;

            int brcnt = 1;
            int cpos = brs;

            while (brcnt > 0)
            {
                cpos = cpos + 1;
                char c = objectString[cpos];
                if (c == '(')
                    brcnt = brcnt + 1;
                else if (c == ')')
                    brcnt = brcnt - 1;
            }

            int bre = cpos;

            return objectString.Substring(0, brs - 1) +
                $"(GmmiVariables \"{variableName}\" \"{variableValue}\" 2)" +
                objectString.Substring(bre + 1);
        }
    }
}
