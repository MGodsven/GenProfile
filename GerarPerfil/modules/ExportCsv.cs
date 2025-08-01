using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Classes;
using Utils;
using ZwSoft.ZwCAD.EditorInput;

namespace GerarPerfil.modules
{
    internal class ExportCsv
    {
        private void WriteData(string path, List<Data> datas)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    foreach (Data currentData in datas)
                    {
                        foreach (var field in typeof(Data).GetProperties().Where(p => (p.Name != "Position" && p.Name != "ProfilePosition" && p.Name != "PlanPosition")))
                        {
                            sw.Write(field.GetValue(currentData));
                            sw.Write(";");
                        }

                        // Writing data Position
                        sw.Write(currentData.PlanPosition.X + ";" + currentData.PlanPosition.Y + ";");
                        sw.Write(sw.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving .csv file on: {path}\nError: {ex.Message}");
            }
        }

        public void Main(Profile profile, Drawing drawing)
        {
            PromptResult gerarExcel = Utils.GetType.GetKeyWords("Export to a .csv file?", new[] { "Yes", "No" }, false, "Yes");
            if (gerarExcel.Status == PromptStatus.Error)
                return;

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "ExcelGerado.csv");

            if (gerarExcel.StringResult == "Yes")
            {
                drawing.Editor.WriteMessage("Saving .csv file on: " + path);
                try
                {
                    WriteData(path, profile.Datas);
                    drawing.Editor.WriteMessage(".");
                }
                catch (System.Exception ex)
                {
                    drawing.Editor.WriteMessage(ex.Message);
                }
            }
        }
    }
}
