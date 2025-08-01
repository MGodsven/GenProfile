using System.Collections.Generic;
using System.Linq;
using Classes;
using GerarPerfil.components;
using Utils;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;

namespace GerarPerfil.modules
{
    internal class DrawProfile
    {
        private List<Line> GetRefLines(Profile profile)
        {
            List<Line> lines = new List<Line>(profile.Invert.NumberOfVertices);

            for (int i = 0; i < profile.Invert.NumberOfVertices; i++)
            {
                var position = profile.Invert.GetPoint3dAt(i);
                lines.Add(Utils.GetType.GetLine(position, new Point3d(position.X, profile.BaseLine.StartPoint.Y, position.Z)));
            }

            return lines;
        }

        public void PutProfileDataToDrawing(Profile profile, Drawing drawing)
        {
            TransWrapper trans = new TransWrapper(drawing);

                trans.DrawLine(GetRefLines(profile));
                
                foreach (Data data in profile.Datas)
                {
                    double curHorizontalSpace = profile.BaseLine.StartPoint.Y;

                    foreach (var field in typeof(Data).GetProperties().Where(p => p.Name != "Position" && p.Name != "Equipment" && p.Name != "PlanPosition" && p.Name != "ProfilePosition"))
                    {
                        curHorizontalSpace -= profile.TextSet.TamanhoTexto * profile.HorizontalSpace;

                        var value = field.GetValue(data);
                        var entity = Utils.GetType.GetDBText(profile, value, new Point3d(data.ProfilePosition.X, curHorizontalSpace, data.ProfilePosition.Z));

                        trans.blockRecord.AppendEntity(entity);
                        trans.transaction.AddNewlyCreatedDBObject(entity, true);
                    }
                }

            trans.transaction.Commit();
        }

    }
}
