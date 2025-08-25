using System;
using System.Linq;
using Utils;

/*
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
*/

using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;

using static Utils.Extensions;
using static Utils.GetType;

namespace Classes
{
    public partial class Profile
    {
        public enum LineState
        {
            Lowering,
            Uppering,
            None,
        }
    
        // I just took these numbers from a test that i did, I'll calculate a better number for these two
        // We gotta remember that, there's isn't such a thing is two same station. and since this is based on Math.Round with 2 decimal number, 
        // We just need to know when we have 0 for both cases
        const double MAX_LEFT_DIST_TOLERANCE = 0.889;
        const double MAX_RIGHT_DIST_TOLERANCE_OFF = 0.161;

        public Polyline GeraPontos()
        {
            Point3dCollection pts = new Point3dCollection();
            Point2dCollection plDraw = new Point2dCollection();

            // Gather line by intersection
            using (Line line = new Line())
            {
                int i = 0;

                for (double currentDistance = Invert.StartPoint.X; currentDistance <= Invert.EndPoint.X; currentDistance += SeparadorEstacas)
                {
                    while (i < Invert.NumberOfVertices && Invert.GetPoint2dAt(i).X < currentDistance)
                    {
                        // This double check is done since such small distance is usually on mistake. That won't cover all cases
                        // i.g the user might use a scale that is ridiculously small.
                        double dis = currentDistance - Invert.GetPoint2dAt(i).X;

                        // Need to check for the right side as well
                        if (dis > MAX_LEFT_DIST_TOLERANCE && dis < SeparadorEstacas - MAX_RIGHT_DIST_TOLERANCE_OFF)
                            plDraw.Add(Invert.GetPoint2dAt(i));

                        i++;
                    }

                    line.StartPoint = new Point3d(currentDistance, Invert.StartPoint.Y, 0);
                    line.EndPoint = new Point3d(currentDistance, Invert.StartPoint.Y + 1, 0);

                    line.IntersectWith(Invert, Intersect.ExtendThis, pts, IntPtr.Zero, IntPtr.Zero);

                    Point2d last = new Point2d(pts.ToArray().Last().X, pts.ToArray().Last().Y);

                    if (Invert.GetPoint2dAt(i) == last)
                        i++;

                    plDraw.Add(last);
                }

                while (i < Invert.NumberOfVertices)
                    plDraw.Add((Invert.GetPoint2dAt(i++)));

                return GetPolyline(plDraw);
            }
        }
        public string GeraNumeracaoEstacas(int invertIndex)
        {
            string textString;

            double curDist = Invert.GetPoint3dAt(invertIndex).X - Invert.GetPoint3dAt(0).X;

            if (SeparadorEstacas > 0)
                textString = ((int)(curDist / SeparadorEstacas)).ToString() +
                                  ((int)(curDist % SeparadorEstacas) != 0 ? " + " + ((int)(Math.Round(curDist, 2) % SeparadorEstacas)).ToString() : "");
            else
                textString = invertIndex.ToString();

            return textString;
        }
        public double GetTerrainLevel(int index)
        {
            var position = Invert.GetPoint3dAt(index);

            using (Line line = new Line())
            {
                Point3dCollection pts = new Point3dCollection();

                line.StartPoint = position;
                line.EndPoint = new Point3d(position.X, position.Y + 1, 0);

                line.IntersectWith(Terrain, Intersect.ExtendThis, pts, IntPtr.Zero, IntPtr.Zero);

                if (pts.Count == 0)        
                    return double.NaN; 
                
                double yTerreno = pts.ToArray().First().Y;

                // Escreve nivel de terreno
                return Math.Round((yTerreno - BaseLine.StartPoint.Y) / Scale + ValorInicial, 3);
            }
        }
        public double GetInvertLevel(int index)
        {
            double curNivel = (Invert.GetPoint3dAt(index).Y - BaseLine.StartPoint.Y) / Scale + ValorInicial;

            return Math.Round(curNivel, 3);
        }
        public double GetDepth(int index)
        {
            return Math.Round(Datas[index].TerrainLevel - Datas[index].InvertLevel, 2);
        }

        public double GetSlope(Data firstData, Data lastData)
        {
            return Math.Round(
                Math.Abs(
                    (firstData.InvertLevel - lastData.InvertLevel) / (firstData.GradualDistance - lastData.GradualDistance)), 4);
        }
        public double GetLength(Data firstData, Data lastData)
        {
            return Math.Round(firstData.GradualDistance - lastData.GradualDistance, 2);
        }
        public double GetLength(double lastPosition, double firstPosition)
        {
            return Math.Round(lastPosition - firstPosition, 2);
        }
    }
}
