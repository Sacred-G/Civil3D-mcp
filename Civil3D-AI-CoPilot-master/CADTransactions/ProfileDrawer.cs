using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

namespace Cad_AI_Agent.CADTransactions
{
    public static class ProfileDrawer
    {
        public static void Draw(Document doc, double offsetX, double offsetY)
        {
            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            if (civilDoc == null) return;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                doc.Editor.WriteMessage("\n[AI]: Drawing Profile View relative to Alignment start...");

                ObjectIdCollection alignIds = civilDoc.GetAlignmentIds();
                if (alignIds.Count == 0)
                {
                    doc.Editor.WriteMessage("\n[AI Error]: No Alignment found in the drawing!");
                    return;
                }
                ObjectId alignId = alignIds[0];

                // 💡 ვიღებთ Alignment-ის ობიექტს, რომ მისი კოორდინატები წავიკითხოთ
                Alignment align = trans.GetObject(alignId, OpenMode.ForRead) as Alignment;

                ObjectIdCollection surfIds = civilDoc.GetSurfaceIds();
                if (surfIds.Count == 0)
                {
                    doc.Editor.WriteMessage("\n[AI Error]: No TIN Surface found in the drawing!");
                    return;
                }
                ObjectId surfId = surfIds[0];

                ObjectId layerId = db.LayerZero;
                ObjectId styleId = civilDoc.Styles.ProfileStyles.Count > 0 ? civilDoc.Styles.ProfileStyles[0] : ObjectId.Null;
                ObjectId labelSetId = civilDoc.Styles.LabelSetStyles.ProfileLabelSetStyles.Count > 0 ? civilDoc.Styles.LabelSetStyles.ProfileLabelSetStyles[0] : ObjectId.Null;

                string profileName = "AI_SurfaceProfile_" + DateTime.Now.ToString("HHmmss");
                Profile.CreateFromSurface(profileName, alignId, surfId, layerId, styleId, labelSetId);

                // 💡 აქ ვთვლით რელატიურ კოორდინატებს!
                double startX = 0, startY = 0;
                try
                {
                    // ვიღებთ Alignment-ის საწყისი სადგურის (X, Y) კოორდინატებს
                    align.PointLocation(align.StartingStation, 0, ref startX, ref startY);
                }
                catch
                {
                    // თუ რამე შეცდომა მოხდა, დავტოვებთ 0,0 -ზე
                }

                // საწყის კოორდინატებს ვუმატებთ ოფსეტებს (მაგ. X+0, Y+400)
                Point3d insertPt = new Point3d(startX + offsetX, startY + offsetY, 0);

                ObjectId viewBandSetId = civilDoc.Styles.ProfileViewBandSetStyles.Count > 0 ? civilDoc.Styles.ProfileViewBandSetStyles[0] : ObjectId.Null;

                string viewName = "AI_ProfileView_" + DateTime.Now.ToString("HHmmss");

                if (viewBandSetId != ObjectId.Null)
                    ProfileView.Create(alignId, insertPt, viewName, viewBandSetId, civilDoc.Styles.ProfileViewStyles[0]);
                else
                    ProfileView.Create(alignId, insertPt); // Fallback

                trans.Commit();
                doc.Editor.WriteMessage("\n[AI Success]: Profile View successfully created at Alignment Start Location!");
            }
        }
    }
}