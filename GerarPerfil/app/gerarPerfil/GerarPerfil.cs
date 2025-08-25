using Classes;
using System;
using Utils;

// Autodesk
/*
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;
*/

using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Runtime;
using GerarPerfil.modules;
using Exception = System.Exception;
using GerarPerfil.components;

namespace GerarPerfil
{
    public class GerarPerfil
    {
        const double TEXT_ROTATION = 90 * Math.PI / 180;
        const double HORIZONTAL_SPACE = 20;

        [CommandMethod("GerarPerfil")]
        public static void Main()
        {
            Drawing currentDrawing = Drawing.GetInstance();
            
            
            PromptEntityResult giRes = Utils.GetType.SelectPolyline("Select the profile invert polyline:");

            if (giRes.Status != PromptStatus.OK)
                return;

            PromptEntityResult terrenoRes = Utils.GetType.SelectPolyline("Select the profile terrain polyline:");

            if (terrenoRes.Status != PromptStatus.OK)
                return;

            PromptEntityResult baseRes = Utils.GetType.SelectLine("Select the profile base line:");

            if (baseRes.Status != PromptStatus.OK)
                return;

            PromptDoubleResult scaleRes = Utils.GetType.TypeDouble("Scale: ", 10);

            if (baseRes.Status != PromptStatus.OK)
                return;

            PromptDoubleResult valorInicial = Utils.GetType.TypeDouble("Type the profile baseLine level:");

            if (valorInicial.Status != PromptStatus.OK)
                return;

            PromptDoubleResult tamanhoTexto = Utils.GetType.TypeDouble("Type the font-height:");

            if (tamanhoTexto.Status != PromptStatus.OK)
                return;

            PromptDoubleResult distancia = Utils.GetType.TypeDouble("Increase profile station counter every how much distance?", null, true);

            if (distancia.Status != PromptStatus.OK)
                return;

            PromptResult geraPontos = Utils.GetType.GetKeyWords("Do you want to generate the complete invert polyline?", new[] {"Yes", "No"}, false, "Yes");

            if (geraPontos.Status == PromptStatus.Error)
                return;

            TransWrapper trans = new TransWrapper(currentDrawing);

            Profile profile = new Profile(
                trans.transaction.GetObject(giRes.ObjectId, OpenMode.ForWrite) as Polyline,
                trans.transaction.GetObject(terrenoRes.ObjectId, OpenMode.ForRead) as Polyline,
                trans.transaction.GetObject(baseRes.ObjectId, OpenMode.ForRead) as Line,
                scaleRes.Value,
                new Text(tamanhoTexto.Value, TEXT_ROTATION),
                distancia.Value,
                valorInicial.Value,
                HORIZONTAL_SPACE
            );

            if (geraPontos.StringResult == "Yes" || geraPontos.Status == PromptStatus.None)
            {
                profile.Invert = profile.GeraPontos();

                trans.DrawPolyline(profile.Invert);
            }

            trans.transaction.Commit();
            trans.Dispose();
            profile.PopulateData();

            // PLUG-IN AREA
            try
                {
                DrawProfile drawProfile = new DrawProfile();
                drawProfile.PutProfileDataToDrawing(profile, currentDrawing);
                
                SyncProfileToPlan syncProfileToPlan = new SyncProfileToPlan();
                syncProfileToPlan.Main(profile, currentDrawing);

                DrawEquipments drawEquipments = new DrawEquipments();
                drawEquipments.AddBlocks(profile.Datas, currentDrawing, DrawEquipments.Place.Profile);
                drawEquipments.AddBlocks(profile.Datas, currentDrawing, DrawEquipments.Place.Plan);

                ExportCsv exportCsv = new ExportCsv();
                exportCsv.Main(profile, currentDrawing);
            }
            catch (Exception ex)
            {
                currentDrawing.Editor.WriteMessage("[ERROR] " + ex.Message);
            }
        }
    }
}
