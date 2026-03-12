using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Cad_AI_Agent.CADTransactions
{
    public class LineDrawer
    {
        // ეს მეთოდი მიიღებს დოკუმენტს და კოორდინატებს
        public static void Draw(Document doc, double startX, double startY, double endX, double endY)
        {
            // დოკუმენტის დაბლოკვა აუცილებელია გარედან ბრძანების გაშვებისას
            using (DocumentLock docLock = doc.LockDocument())
            {
                using (Transaction tr = doc.TransactionManager.StartTransaction())
                {
                    BlockTable bt = (BlockTable)tr.GetObject(doc.Database.BlockTableId, OpenMode.ForRead);
                    BlockTableRecord btr = (BlockTableRecord)tr.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                    Point3d startPt = new Point3d(startX, startY, 0);
                    Point3d endPt = new Point3d(endX, endY, 0);

                    using (Line acLine = new Line(startPt, endPt))
                    {
                        btr.AppendEntity(acLine);
                        tr.AddNewlyCreatedDBObject(acLine, true);
                    }

                    tr.Commit();
                    doc.Editor.WriteMessage($"\n[AI Agent] Successfully drew a line from ({startX},{startY}) to ({endX},{endY}).");
                }
            }
        }
    }
}