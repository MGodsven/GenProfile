using System;
using System.Collections.Generic;
using System.IO;
using Classes;
using GerarPerfil.components;
using Utils;
using ZwSoft.ZwCAD.DatabaseServices;

namespace GerarPerfil.modules
{
    internal class DrawEquipments
    {
        public enum Place
        {
            Profile,
            Plan
        }

        private enum TextPos
        {
            Suffix,
            Prefix,
            Overwrite
        }

        private readonly string resourcesRoot;
        private const string RESOURCES_FOLDER = "resources";

        public DrawEquipments()
        {
            resourcesRoot = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        private void ModifyTextBlock(Database db, string clientText, TextPos textPos)
        {
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTableRecord btr = tr.GetObject(db.CurrentSpaceId, OpenMode.ForWrite) as BlockTableRecord;

                foreach (ObjectId id in btr)
                {
                    Entity ent = tr.GetObject(id, OpenMode.ForRead) as Entity;

                    if (ent is DBText text)
                    {
                        text.UpgradeOpen();

                        switch (textPos)
                        {
                            case TextPos.Prefix:
                                text.TextString = clientText + text.TextString;
                                break;
                            case TextPos.Suffix:
                                text.TextString += clientText;
                                break;
                            case TextPos.Overwrite:
                                text.TextString = clientText;
                                break;
                        }
                    } else if (ent is MText mText)
                    {
                        mText.UpgradeOpen();

                        switch (textPos)
                        {
                            case TextPos.Prefix:
                                mText.Contents = clientText + mText.Contents;
                                break;
                            case TextPos.Suffix:
                                mText.Contents += clientText;
                                break;
                            case TextPos.Overwrite:
                                mText.Contents = clientText;
                                break;
                        }
                    }
                }
                tr.Commit();
            }
        }

        public void AddBlocks(List<Data> datas, Drawing drawing, Place place)
        {
            TransWrapper trans = new TransWrapper(drawing);

            int dischargeCounter = 0;
            int airventCounter = 0;
            foreach (Data data in datas)
            {
                if (data.Equipment != Equipment.None)
                {
                    string path = Path.Combine(resourcesRoot, RESOURCES_FOLDER, place.ToString(), data.Equipment.ToString() + ".dwg");

                    using (Database db = new Database(false, true))
                    {
                        db.ReadDwgFile(path, FileOpenMode.OpenForReadAndReadShare, false, null);

                        if (data.Equipment == Equipment.AirVentValve)
                        {
                            airventCounter++;
                            ModifyTextBlock(db, " " + airventCounter.ToString(), TextPos.Suffix);
                        }
                        else if (data.Equipment == Equipment.Discharge)
                        {
                            dischargeCounter++;
                            ModifyTextBlock(db, " " + dischargeCounter.ToString(), TextPos.Suffix);
                        }

                        ObjectId block = trans.drawing.Database.Insert(data.Equipment.ToString() + Guid.NewGuid().ToString("N"), db, false);

                        if (place == Place.Profile)
                        {
                            trans.AddBlock(data.ProfilePosition, block);
                        }
                        else
                        {
                            trans.AddBlock(data.PlanPosition, block);

                        }
                    }
                }

                if (place == Place.Plan)
                {
                    string path = Path.Combine(resourcesRoot, RESOURCES_FOLDER, place.ToString(), "Station" + ".dwg");

                    using (Database db = new Database(false, true))
                    {
                        db.ReadDwgFile(path, FileOpenMode.OpenForReadAndReadShare, false, null);
                        ModifyTextBlock(db, data.Estaca, TextPos.Overwrite);
                        ObjectId block = trans.drawing.Database.Insert(data.Equipment.ToString() + Guid.NewGuid().ToString("N"), db, false);
                        trans.AddBlock(data.PlanPosition, block);
                    }
                }
            }

            trans.transaction.Commit();
            trans.Dispose();
        }
    }
}
