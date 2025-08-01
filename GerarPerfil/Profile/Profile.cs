using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/*
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
*/

using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;

using Utils;

namespace Classes
{
    public partial class Profile
    {
        public Polyline Invert { set; get; }
        public Polyline Terrain { set; get; }
        public Line BaseLine { set; get; }
        public double Scale { set; get; }
        public Text TextSet { get; set; }
        public double SeparadorEstacas { get; set; }
        public double ValorInicial { get; set; }
        public List<Data> Datas { get; set; }
        public double HorizontalSpace { get; set; }

        public Profile(Polyline invert, Polyline terrain, Line baseLine, double scale, Text textSet, double separadorEstacas, double valorInicial, double horizontalSpace)
        {
            Invert = invert;
            Terrain = terrain;
            BaseLine = baseLine;
            Scale = scale;
            TextSet = textSet;
            SeparadorEstacas = separadorEstacas;
            ValorInicial = valorInicial;
            HorizontalSpace = horizontalSpace;
        }

        public void PopulateData()
        {
            Datas = new List<Data>(Invert.NumberOfVertices);
            double currentDistance = 0;

            LineState state = LineState.None;

            for (int i = 0; i < Invert.NumberOfVertices; i++)
            {
                var invertPostion = Invert.GetPoint3dAt(i);

                var data = new Data
                {
                    ProfilePosition = new Point3d(invertPostion.X, invertPostion.Y, invertPostion.Z),
                    Estaca = GeraNumeracaoEstacas(i),
                    TerrainLevel = GetTerrainLevel(i),
                    InvertLevel = GetInvertLevel(i),
                    GradualDistance = GetLength(invertPostion.X, Invert.StartPoint.X),
                };

                data.CalculateDepth();

                data.Equipment = Equipment.None;

                if (i > 0)
                {
                    data.Slope = GetSlope(data, Datas.Last());
                    data.Length = GetLength(data, Datas.Last());

                    Data lastData = Datas[i - 1];
                    LineState curState = data.InvertLevel < Datas[i - 1].InvertLevel ? LineState.Lowering : LineState.Uppering;

                    if ((state == LineState.Lowering) && (curState == LineState.Uppering))
                    {
                        lastData.Equipment = Equipment.Discharge;
                    }
                    else if ((state == LineState.Uppering) && (curState == LineState.Lowering))
                    {
                        lastData.Equipment = Equipment.SuctionCup;
                    }

                    // There's no need to set the last Value as none, since it's already set
                    state = curState;
                    Datas[i - 1] = lastData;
                }
                else
                {
                    data.Slope = double.NaN;
                    data.Length = double.NaN;
                }

                Datas.Add(data);
                currentDistance += SeparadorEstacas;
            }
        }
    }
}
