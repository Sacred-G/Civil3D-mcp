using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Cad_AI_Agent.CADTransactions
{
    /// <summary>
    /// Draws a slope/grade arrow in plan view:
    ///   - A line shaft from start to end
    ///   - An arrowhead at the end point
    ///   - A DBText label showing the grade percentage, offset perpendicular to the shaft
    /// Default layer: AI_SlopeArrow (green/3).
    /// </summary>
    public static class SlopeArrowDrawer
    {
        private const string Layer = "AI_SlopeArrow";

        /// <param name="x1">Start X</param>
        /// <param name="y1">Start Y</param>
        /// <param name="z1">Start elevation</param>
        /// <param name="x2">End X</param>
        /// <param name="y2">End Y</param>
        /// <param name="z2">End elevation</param>
        /// <param name="textHeight">Height of the grade annotation text (default 2.5 units)</param>
        public static void Draw(Document doc,
            double x1, double y1, double z1,
            double x2, double y2, double z2,
            double textHeight = 2.5)
        {
            double horizDist = Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
            double slopePct = horizDist > 1e-9 ? (z2 - z1) / horizDist * 100.0 : 0.0;
            double shaftAngle = Math.Atan2(y2 - y1, x2 - x1);

            using DocumentLock _ = doc.LockDocument();
            using Transaction tr = doc.Database.TransactionManager.StartTransaction();

            EnsureLayer(doc.Database, tr, Layer, colorIndex: 3); // green

            BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            // Main shaft — drawn in plan view (Z=0)
            var shaft = new Line(new Point3d(x1, y1, 0), new Point3d(x2, y2, 0)) { Layer = Layer };
            btr.AppendEntity(shaft);
            tr.AddNewlyCreatedDBObject(shaft, true);

            // Arrowhead: two barbs at ±150° from the direction of travel
            double arrowLen = textHeight * 1.5;
            double barb1 = shaftAngle + Math.PI * 5.0 / 6.0;
            double barb2 = shaftAngle - Math.PI * 5.0 / 6.0;
            var arrow1 = new Line(
                new Point3d(x2, y2, 0),
                new Point3d(x2 + Math.Cos(barb1) * arrowLen, y2 + Math.Sin(barb1) * arrowLen, 0))
            { Layer = Layer };
            var arrow2 = new Line(
                new Point3d(x2, y2, 0),
                new Point3d(x2 + Math.Cos(barb2) * arrowLen, y2 + Math.Sin(barb2) * arrowLen, 0))
            { Layer = Layer };
            btr.AppendEntity(arrow1); tr.AddNewlyCreatedDBObject(arrow1, true);
            btr.AppendEntity(arrow2); tr.AddNewlyCreatedDBObject(arrow2, true);

            // Grade label — placed at midpoint, offset one text-height perpendicular to shaft
            string gradeText = $"{slopePct:+0.00;-0.00;0.00}%";
            double perpAngle = shaftAngle + Math.PI / 2.0;
            double midX = (x1 + x2) / 2.0 + Math.Cos(perpAngle) * textHeight;
            double midY = (y1 + y2) / 2.0 + Math.Sin(perpAngle) * textHeight;
            var label = new DBText
            {
                Position = new Point3d(midX, midY, 0),
                TextString = gradeText,
                Height = textHeight,
                Rotation = shaftAngle,
                Layer = Layer
            };
            btr.AppendEntity(label);
            tr.AddNewlyCreatedDBObject(label, true);

            tr.Commit();
            doc.Editor.WriteMessage(
                $"\n[AI Agent] Slope arrow: {gradeText} from ({x1:F2},{y1:F2}) to ({x2:F2},{y2:F2}), layer '{Layer}'.");
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
