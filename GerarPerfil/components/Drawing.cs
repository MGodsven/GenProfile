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

        public Drawing()
        {
            Document = Application.DocumentManager.CurrentDocument;
            Editor = Document.Editor;
            Database = Document.Database;
        }
    }
}
