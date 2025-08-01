using System;
using System.Linq;
using ZwSoft.ZwCAD.Geometry;
using Classes;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using System.IO;
using Utils;
using GerarPerfil.components;

namespace GerarPerfil.modules
{
    internal class SyncProfileToPlan
    {
        enum Direction
        {
            StartToEnd = 1,
            EndToStart = -1,
        }

        public void Main(Profile profile, Drawing drawing)
        {
            PromptEntityResult plRes = Utils.GetType.SelectPolyline("Select the profile invert polyline:");

            if (plRes.Status != PromptStatus.OK)
                return;

            TransWrapper tw = new TransWrapper(drawing);

            Polyline pl = tw.transaction.GetObject(plRes.ObjectId, OpenMode.ForRead) as Polyline;

            if (profile.Datas.Last().GradualDistance > pl.Length)
            {
                throw new Exception($"Polyline is not long enough. Profile length: {profile.Datas.Last().GradualDistance} | Selected Poyline length: {pl.Length}");
            }

            tw.Dispose();

            Direction dir = GetDir(pl, drawing);
            string resourcesRoot = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            SetPlanData(profile, pl, dir);
        }

        private static void SetPlanData(Profile profile, Polyline pl, Direction direction)
        {
            double distance = 0;
            int multipler = 0;

            switch (direction)
            {
                case Direction.StartToEnd:
                    distance = 0;
                    multipler = 1;
                    break;
                case Direction.EndToStart:
                    distance = pl.Length;
                    multipler = -1;
                    break;
            }



            // It is safe to assume the data ordem is in the correct order (Crescent)
            for (int i = 0; i < profile.Datas.Count; i++)
            {
                Data data = profile.Datas[i];
                double curLength = data.Length;

                if (double.IsNaN(data.Length))
                    curLength = 0; 

                distance += (curLength * multipler);
                data.PlanPosition = pl.GetPointAtDist(distance);

                profile.Datas[i] = data;
            }
        }

        private static Direction GetDir(Polyline pl, Drawing drawing)
        {
            ZoomPoint(drawing.Editor, pl.StartPoint);
            
            PromptResult geraPontos = Utils.GetType.GetKeyWords("Is this location the PolyLine starting point?", new[] { "Yes", "No" }, false, "Yes");

            if (geraPontos.Status == PromptStatus.Error)
            {
                throw new Exception("Failed to get User input.");
            }

            if (geraPontos.StringResult == "Yes" || geraPontos.Status == PromptStatus.None)
            {
                return Direction.StartToEnd; 
            } else
            {
                return Direction.EndToStart;
            }
        }

        private static void ZoomPoint(Editor ed, Point3d center)
        {
            ViewTableRecord view = new ViewTableRecord();

            Point2d center2d = new Point2d(center.X, center.Y);

            view.CenterPoint = center2d;
            view.Height = 10;
            view.Width = 10;

            ed.SetCurrentView(view);
        }

        private static void ZoomWin(Editor ed, Point3d min, Point3d max)
        {
            Point2d min2d = new Point2d(min.X, min.Y);
            Point2d max2d = new Point2d(max.X, max.Y);


            ViewTableRecord view = new ViewTableRecord();

            view.CenterPoint = min2d + ((max2d - min2d) / 2.0);

            view.Height = max2d.Y - min2d.Y;
            view.Width = max2d.X - min2d.X;

            ed.SetCurrentView(view);
        }
    }
}
