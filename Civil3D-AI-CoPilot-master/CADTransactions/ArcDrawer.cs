using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Cad_AI_Agent.CADTransactions
{
    /// <summary>
    /// Draws an arc by start point, center, and end point.
    /// The arc sweeps counter-clockwise from startAngle to endAngle.
    /// Default layer: AI_Arc (magenta).
    /// </summary>
    public static class ArcDrawer
    {
        private const string Layer = "AI_Arc";

        public static void Draw(Document doc, double startX, double startY,
            double centerX, double centerY, double endX, double endY)
        {
            double radius = Math.Sqrt(Math.Pow(startX - centerX, 2) + Math.Pow(startY - centerY, 2));
            double startAngle = Math.Atan2(startY - centerY, startX - centerX);
            double endAngle = Math.Atan2(endY - centerY, endX - centerX);

            using DocumentLock _ = doc.LockDocument();
            using Transaction tr = doc.Database.TransactionManager.StartTransaction();

            EnsureLayer(doc.Database, tr, Layer, colorIndex: 6); // magenta

            BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            var arc = new Arc(new Point3d(centerX, centerY, 0), radius, startAngle, endAngle)
            {
                Layer = Layer
            };
            btr.AppendEntity(arc);
            tr.AddNewlyCreatedDBObject(arc, true);

            tr.Commit();
            doc.Editor.WriteMessage(
                $"\n[AI Agent] Arc drawn: center ({centerX:F2}, {centerY:F2}), radius {radius:F2}, layer '{Layer}'.");
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
