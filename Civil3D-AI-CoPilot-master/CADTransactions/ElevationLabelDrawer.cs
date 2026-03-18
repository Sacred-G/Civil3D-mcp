using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Cad_AI_Agent.CADTransactions
{
    /// <summary>
    /// Places a spot elevation annotation at a plan coordinate:
    /// a small cross-hair marker plus a DBText label showing the elevation value.
    /// Default layer: AI_ElevLabel (white/7).
    /// </summary>
    public static class ElevationLabelDrawer
    {
        private const string Layer = "AI_ElevLabel";

        public static void Draw(Document doc, double x, double y, double elevation,
            string prefix = "ELEV=", double textHeight = 2.5)
        {
            using DocumentLock _ = doc.LockDocument();
            using Transaction tr = doc.Database.TransactionManager.StartTransaction();

            EnsureLayer(doc.Database, tr, Layer, colorIndex: 7); // white

            BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            // Small cross-hair marker centered on the survey point
            double half = textHeight * 0.5;
            var crossH = new Line(new Point3d(x - half, y, 0), new Point3d(x + half, y, 0)) { Layer = Layer };
            var crossV = new Line(new Point3d(x, y - half, 0), new Point3d(x, y + half, 0)) { Layer = Layer };
            btr.AppendEntity(crossH); tr.AddNewlyCreatedDBObject(crossH, true);
            btr.AppendEntity(crossV); tr.AddNewlyCreatedDBObject(crossV, true);

            // Text label offset slightly to the upper-right of the marker
            string labelText = $"{prefix}{elevation:F2}";
            var text = new DBText
            {
                Position = new Point3d(x + half * 1.5, y + half * 0.5, 0),
                TextString = labelText,
                Height = textHeight,
                Layer = Layer
            };
            btr.AppendEntity(text);
            tr.AddNewlyCreatedDBObject(text, true);

            tr.Commit();
            doc.Editor.WriteMessage(
                $"\n[AI Agent] Elevation label placed at ({x:F2}, {y:F2}): {labelText}, layer '{Layer}'.");
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
