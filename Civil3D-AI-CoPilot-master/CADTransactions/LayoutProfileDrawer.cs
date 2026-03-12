using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

namespace Cad_AI_Agent.CADTransactions
{
    public static class LayoutProfileDrawer
    {
        public static void Draw(Document doc, double[] pviData, string baseName = "AI_DesignProfile")
        {
            if (pviData == null || pviData.Length < 4) return;

            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            if (civilDoc == null) return;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                ObjectIdCollection alignIds = civilDoc.GetAlignmentIds();
                if (alignIds.Count == 0) return;
                ObjectId alignId = alignIds[0];

                // 💡 Alignment-ის ამოღება დაგვჭირდება, რომ მისი კოორდინატები გავიგოთ
                Alignment align = trans.GetObject(alignId, OpenMode.ForRead) as Alignment;

                ObjectId layerId = db.LayerZero;
                ObjectId styleId = civilDoc.Styles.ProfileStyles.Count > 0 ? civilDoc.Styles.ProfileStyles[0] : ObjectId.Null;
                ObjectId labelSetId = civilDoc.Styles.LabelSetStyles.ProfileLabelSetStyles.Count > 0 ? civilDoc.Styles.LabelSetStyles.ProfileLabelSetStyles[0] : ObjectId.Null;

                string profileName = baseName + "_" + DateTime.Now.ToString("HHmmss");
                ObjectId profileId = Profile.CreateByLayout(profileName, alignId, layerId, styleId, labelSetId);
                Profile layoutProfile = trans.GetObject(profileId, OpenMode.ForWrite) as Profile;

                // ტანგესების დასმის სტაბილური მეთოდი
                Point2d? prevPt = null;
                for (int i = 0; i < pviData.Length; i += 2)
                {
                    if (i + 1 < pviData.Length)
                    {
                        Point2d currPt = new Point2d(pviData[i], pviData[i + 1]);
                        if (prevPt.HasValue)
                        {
                            layoutProfile.Entities.AddFixedTangent(prevPt.Value, currPt);
                        }
                        prevPt = currPt;
                    }
                }

                // 💡 Profile View (ცხრილის) დახატვა რეალურ კოორდინატებზე!
                double startX = 0, startY = 0;
                align.PointLocation(align.StartingStation, 0, ref startX, ref startY);

                // ვსვამთ პროფილს გზის დასაწყისიდან 250 მეტრით მაღლა (Y ღერძზე)
                Point3d insertPt = new Point3d(startX, startY + 250, 0);

                ObjectId pvStyleId = civilDoc.Styles.ProfileViewStyles.Count > 0 ? civilDoc.Styles.ProfileViewStyles[0] : ObjectId.Null;
                ObjectId pvBandSetId = civilDoc.Styles.ProfileViewBandSetStyles.Count > 0 ? civilDoc.Styles.ProfileViewBandSetStyles[0] : ObjectId.Null;

                if (pvStyleId != ObjectId.Null && pvBandSetId != ObjectId.Null)
                {
                    string pvName = "AI_PV_Manual_" + DateTime.Now.ToString("HHmmss");
                    try
                    {
                        ProfileView.Create(civilDoc, pvName, pvBandSetId, alignId, insertPt);
                    }
                    catch { }
                }

                trans.Commit();
                doc.Editor.Regen();
            }
        }
    }
}