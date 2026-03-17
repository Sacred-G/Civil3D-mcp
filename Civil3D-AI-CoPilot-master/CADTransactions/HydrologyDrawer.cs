using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.ApplicationServices;

namespace Cad_AI_Agent.CADTransactions
{
    /// <summary>
    /// Draws hydrology analysis results as Civil 3D geometry:
    /// flow path polylines, watershed boundaries, and outlet/low-point markers.
    /// </summary>
    public static class HydrologyDrawer
    {
        private const string FlowPathLayer = "AI_Hydro_FlowPath";
        private const string WatershedLayer = "AI_Hydro_Watershed";
        private const string OutletLayer = "AI_Hydro_Outlet";

        /// <summary>
        /// Draws a traced flow path as a 3D polyline on the AI_Hydro_FlowPath layer (cyan).
        /// </summary>
        public static void DrawFlowPath(Document doc, List<(double X, double Y, double Elevation)> points, string label = "")
        {
            if (points == null || points.Count < 2)
            {
                doc.Editor.WriteMessage("\n[AI Hydro]: Not enough flow path points to draw a polyline (need at least 2).");
                return;
            }

            using DocumentLock _ = doc.LockDocument();
            using Transaction trans = doc.Database.TransactionManager.StartTransaction();

            EnsureLayer(doc.Database, trans, FlowPathLayer, colorIndex: 4); // cyan

            BlockTableRecord btr = (BlockTableRecord)trans.GetObject(
                doc.Database.CurrentSpaceId, OpenMode.ForWrite);

            var pts3d = new Point3dCollection();
            foreach (var p in points)
                pts3d.Add(new Point3d(p.X, p.Y, p.Elevation));

            var pline = new Polyline3d(Poly3dType.SimplePoly, pts3d, false)
            {
                Layer = FlowPathLayer
            };
            btr.AppendEntity(pline);
            trans.AddNewlyCreatedDBObject(pline, true);

            trans.Commit();
            doc.Editor.WriteMessage($"\n[AI Hydro]: Flow path drawn — {points.Count} points, layer '{FlowPathLayer}'." +
                (string.IsNullOrEmpty(label) ? "" : $" ({label})"));
        }

        /// <summary>
        /// Draws a watershed boundary as a closed 2D polyline on the AI_Hydro_Watershed layer (yellow).
        /// </summary>
        public static void DrawWatershedBoundary(Document doc, List<(double X, double Y)> boundaryPoints, double approximateArea = 0)
        {
            if (boundaryPoints == null || boundaryPoints.Count < 3)
            {
                doc.Editor.WriteMessage("\n[AI Hydro]: Not enough boundary points to draw a watershed polygon (need at least 3).");
                return;
            }

            using DocumentLock _ = doc.LockDocument();
            using Transaction trans = doc.Database.TransactionManager.StartTransaction();

            EnsureLayer(doc.Database, trans, WatershedLayer, colorIndex: 2); // yellow

            BlockTableRecord btr = (BlockTableRecord)trans.GetObject(
                doc.Database.CurrentSpaceId, OpenMode.ForWrite);

            var pline = new Polyline();
            pline.Layer = WatershedLayer;

            for (int i = 0; i < boundaryPoints.Count; i++)
                pline.AddVertexAt(i, new Point2d(boundaryPoints[i].X, boundaryPoints[i].Y), 0, 0, 0);

            pline.Closed = true;
            btr.AppendEntity(pline);
            trans.AddNewlyCreatedDBObject(pline, true);

            trans.Commit();
            string areaNote = approximateArea > 0
                ? $", approx. area = {approximateArea:F0} sq units"
                : string.Empty;
            doc.Editor.WriteMessage($"\n[AI Hydro]: Watershed boundary drawn — {boundaryPoints.Count} vertices{areaNote}, layer '{WatershedLayer}'.");
        }

        /// <summary>
        /// Places a COGO point at the outlet/low-point and draws a small circle marker.
        /// </summary>
        public static void MarkOutletPoint(Document doc, double x, double y, double elevation, string description = "HYDRO_OUTLET")
        {
            // Place COGO point
            CogoPointDrawer.Draw(doc, x, y, elevation, description);

            // Draw a small circle as a visual marker
            using DocumentLock _ = doc.LockDocument();
            using Transaction trans = doc.Database.TransactionManager.StartTransaction();

            EnsureLayer(doc.Database, trans, OutletLayer, colorIndex: 1); // red

            BlockTableRecord btr = (BlockTableRecord)trans.GetObject(
                doc.Database.CurrentSpaceId, OpenMode.ForWrite);

            // Marker radius = 2 units (reasonable default — engineer can rescale)
            var circle = new Circle(new Point3d(x, y, elevation), Vector3d.ZAxis, 2.0)
            {
                Layer = OutletLayer
            };
            btr.AppendEntity(circle);
            trans.AddNewlyCreatedDBObject(circle, true);

            trans.Commit();
            doc.Editor.WriteMessage($"\n[AI Hydro]: Outlet point marked at ({x:F2}, {y:F2}, elev {elevation:F2}), layer '{OutletLayer}'.");
        }

        private static void EnsureLayer(Database db, Transaction trans, string layerName, short colorIndex)
        {
            var lt = (LayerTable)trans.GetObject(db.LayerTableId, OpenMode.ForRead);
            if (lt.Has(layerName)) return;

            lt.UpgradeOpen();
            var ltr = new LayerTableRecord
            {
                Name = layerName,
                Color = Color.FromColorIndex(ColorMethod.ByAci, colorIndex)
            };
            lt.Add(ltr);
            trans.AddNewlyCreatedDBObject(ltr, true);
        }
    }
}
