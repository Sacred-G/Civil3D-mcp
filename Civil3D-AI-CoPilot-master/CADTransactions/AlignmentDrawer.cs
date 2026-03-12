using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

namespace Cad_AI_Agent.CADTransactions
{
    public static class AlignmentDrawer
    {
        public static void Draw(Document doc, double[] coords, string baseName = "AI_Alignment")
        {
            if (coords == null || coords.Length < 4) return;

            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            if (civilDoc == null) return;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // 1. ჯერ ვხატავთ პოლილაინს წვეროების (PI) კოორდინატებით
                BlockTable bt = (BlockTable)trans.GetObject(db.BlockTableId, OpenMode.ForRead);
                BlockTableRecord btr = (BlockTableRecord)trans.GetObject(bt[BlockTableRecord.ModelSpace], OpenMode.ForWrite);

                Polyline pline = new Polyline();
                for (int i = 0; i < coords.Length; i += 2)
                {
                    if (i + 1 < coords.Length)
                    {
                        pline.AddVertexAt(i / 2, new Point2d(coords[i], coords[i + 1]), 0, 0, 0);
                    }
                }
                ObjectId plineId = btr.AppendEntity(pline);
                trans.AddNewlyCreatedDBObject(pline, true);

                // 2. პოლილაინს ვაქცევთ Civil 3D Alignment-ად
                ObjectId styleId = civilDoc.Styles.AlignmentStyles[0];
                ObjectId labelSetId = civilDoc.Styles.LabelSetStyles.AlignmentLabelSetStyles[0];

                string uniqueName = baseName + "_" + DateTime.Now.ToString("HHmmss");

                // პარამეტრები Alignment-ისთვის
                PolylineOptions plOpts = new PolylineOptions();
                plOpts.AddCurvesBetweenTangents = true; // ამატებს ავტომატურ მოსახვევებს
                plOpts.EraseExistingEntities = true;    // შლის დროებით პოლილაინს
                plOpts.PlineId = plineId;               // ვაწვდით ჩვენი პოლილაინის ID-ს!

                // ვქმნით Alignment-ს
                Alignment.Create(civilDoc, plOpts, uniqueName, ObjectId.Null, db.LayerZero, styleId, labelSetId);

                trans.Commit();
            }
        }
    }
}