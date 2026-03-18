using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Cad_AI_Agent.CADTransactions
{
    /// <summary>
    /// Draws a closed rectangular 2D polyline in model space.
    /// Default layer: AI_Rectangle (blue). Accepts an optional layer override.
    /// </summary>
    public static class RectangleDrawer
    {
        private const string DefaultLayer = "AI_Rectangle";

        public static void Draw(Document doc, double x1, double y1, double x2, double y2, string? layerName = null)
        {
            string targetLayer = layerName ?? DefaultLayer;

            using DocumentLock _ = doc.LockDocument();
            using Transaction tr = doc.Database.TransactionManager.StartTransaction();

            EnsureLayer(doc.Database, tr, targetLayer, colorIndex: 5); // blue

            BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            var pline = new Polyline();
            pline.Layer = targetLayer;
            pline.AddVertexAt(0, new Point2d(x1, y1), 0, 0, 0);
            pline.AddVertexAt(1, new Point2d(x2, y1), 0, 0, 0);
            pline.AddVertexAt(2, new Point2d(x2, y2), 0, 0, 0);
            pline.AddVertexAt(3, new Point2d(x1, y2), 0, 0, 0);
            pline.Closed = true;

            btr.AppendEntity(pline);
            tr.AddNewlyCreatedDBObject(pline, true);

            tr.Commit();

            double width = Math.Abs(x2 - x1);
            double height = Math.Abs(y2 - y1);
            doc.Editor.WriteMessage($"\n[AI Agent] Rectangle drawn: {width:F2} x {height:F2} units on layer '{targetLayer}'.");
        }

        private static void EnsureLayer(Database db, Transaction tr, string name, short colorIndex)
        {
            var lt = (LayerTable)tr.GetObject(db.LayerTableId, OpenMode.ForRead);
            if (lt.Has(name)) return;
            lt.UpgradeOpen();
            var ltr = new LayerTableRecord
            {
                Name = name,
                Color = Color.FromColorIndex(ColorMethod.ByAci, colorIndex)
            };
            lt.Add(ltr);
            tr.AddNewlyCreatedDBObject(ltr, true);
        }
    }
}
