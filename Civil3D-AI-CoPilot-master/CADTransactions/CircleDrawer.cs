using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Cad_AI_Agent.CADTransactions
{
    public class CircleDrawer
    {
        // იღებს ცენტრის X, Y კოორდინატებს და რადიუსს
        public static void Draw(Document doc, double centerX, double centerY, double radius)
        {
            using (DocumentLock docLock = doc.LockDocument())
            {
                using (Transaction tr = doc.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    Point3d centerPt = new Point3d(centerX, centerY, 0);

                    using (Circle acCirc = new Circle())
                    {
                        acCirc.Center = centerPt;
                        acCirc.Radius = radius;
                        btr.AppendEntity(acCirc);
                        tr.AddNewlyCreatedDBObject(acCirc, true);
                    }

                    tr.Commit();
                    doc.Editor.WriteMessage($"\n[AI Agent] Successfully drew a circle at ({centerX},{centerY}) with radius {radius}.");
                }
            }
        }
    }
}