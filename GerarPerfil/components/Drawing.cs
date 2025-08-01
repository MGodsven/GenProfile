using System.Collections.Generic;
using System.IO;
using GerarPerfil.components;


/*
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
*/

using ZwSoft.ZwCAD.ApplicationServices;
using ZwSoft.ZwCAD.DatabaseServices;
using ZwSoft.ZwCAD.EditorInput;
using ZwSoft.ZwCAD.Geometry;


namespace Classes
{
    public sealed class Drawing
    {
        public Document Document { get; }
        public Editor Editor { get; }
        public Database Database { get; }
        static Drawing _instance;

        private Drawing(Document currentDocument)
        {
            Document = currentDocument;
            Editor = currentDocument.Editor;
            Database = currentDocument.Database;
        }

        public static Drawing GetInstance()
        {
            if (_instance == null)
            {
                _instance = new Drawing(Application.DocumentManager.CurrentDocument);
            }

            return _instance;
        }
    }
}
