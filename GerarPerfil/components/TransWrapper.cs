using System;
using System.Collections.Generic;
using System.IO;
using Classes;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.Geometry;

namespace GerarPerfil.components
{
    internal class TransWrapper : IDisposable
    {
        public BlockTable blockTable;
        public BlockTableRecord blockRecord;
        public Transaction transaction;
        public Drawing drawing;

        public TransWrapper(Drawing drawing)
        {
            transaction = drawing.Document.TransactionManager.StartTransaction();
            blockTable = transaction.GetObject(drawing.Database.BlockTableId, OpenMode.ForRead) as BlockTable;
            blockRecord = transaction.GetObject(blockTable[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;
            this.drawing = drawing;
        }

        public ObjectId GetBlock(string path, string name)
        {
            Database newDb = new Database(false, true);
            BlockTable bt = (BlockTable)transaction.GetObject(drawing.Database.BlockTableId, OpenMode.ForRead);

            if (!blockTable.Has(name))
            {
                if (!File.Exists(path))
                    throw new FileNotFoundException("Couldn't find specified file on: " + path);
                
                newDb.ReadDwgFile(path, FileOpenMode.OpenForReadAndReadShare, false, null);
                return drawing.Database.Insert(name, newDb, false);
            } else
            {
                return blockTable[name];
            }
           
        }

        public ObjectId MergeDataBase(Database db, string name)
        {
            return drawing.Database.Insert(name, db, false);
        }


        public void AddBlock(Point3d pos, ObjectId block)
        {
            using (BlockReference acBlkRef = new BlockReference(pos, block))
            {
                blockRecord.AppendEntity(acBlkRef);
                transaction.AddNewlyCreatedDBObject(acBlkRef, true);
            }
        }


        public void DrawPolyline(Polyline polyline)
        {
            blockRecord.AppendEntity(polyline);
            transaction.AddNewlyCreatedDBObject(polyline, true);
        }

        public void DrawLine(Point3d start, Point3d end)
        {
            Line line = new Line(start, end);
            blockRecord.AppendEntity(line);
            transaction.AddNewlyCreatedDBObject(line, true);
        }
        public void DrawCircle(Point3d center, double radius)
        {
            Circle circle = new Circle
            {
                Center = center,
                Radius = radius
            };

            blockRecord.AppendEntity(circle);
            transaction.AddNewlyCreatedDBObject(circle, true);
        }

        public void DrawLine(List<Line> lines)
        {
            foreach (Line line in lines)
            {
                blockRecord.AppendEntity(line);
                transaction.AddNewlyCreatedDBObject(line, true);
            }
        }

        ~TransWrapper()
        {
            Free();
        }

        private void Free()
        {
            blockTable.Dispose();
            blockRecord.Dispose(); 
            transaction.Dispose();
        }

        public void Dispose()
        {
            Free();
            GC.SuppressFinalize(this);
        }
    }
}
