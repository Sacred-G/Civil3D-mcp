using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Cad_AI_Agent.CADTransactions
{
    /// <summary>
    /// Draws a closed 2D polygon from an array of XY coordinate pairs.
    /// Used for site boundaries, right-of-way limits, property lines, and survey extents.
    /// Default layer: AI_Boundary (red). Accepts an optional layer override.
    /// </summary>
    public static class BoundaryDrawer
    {
        private const string DefaultLayer = "AI_Boundary";

        /// <param name="coords">Flat array of XY pairs: [X1, Y1, X2, Y2, ..., Xn, Yn]</param>
        public static void Draw(Document doc, double[] coords, string? layerName = null)
        {
            string targetLayer = layerName ?? DefaultLayer;
            int vertexCount = coords.Length / 2;

            using DocumentLock _ = doc.LockDocument();
            using Transaction tr = doc.Database.TransactionManager.StartTransaction();

            EnsureLayer(doc.Database, tr, targetLayer, colorIndex: 1); // red

            BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
            BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

            var pline = new Polyline();
            pline.Layer = targetLayer;
            for (int i = 0; i < vertexCount; i++)
                pline.AddVertexAt(i, new Point2d(coords[i * 2], coords[i * 2 + 1]), 0, 0, 0);
            pline.Closed = true;

            btr.AppendEntity(pline);
            tr.AddNewlyCreatedDBObject(pline, true);

            tr.Commit();
            doc.Editor.WriteMessage(
                $"\n[AI Agent] Boundary polygon drawn: {vertexCount} vertices, layer '{targetLayer}'.");
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
