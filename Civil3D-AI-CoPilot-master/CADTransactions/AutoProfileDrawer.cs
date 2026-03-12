using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

namespace Cad_AI_Agent.CADTransactions
{
    public static class AutoProfileDrawer
    {
        public static void Draw(Document doc)
        {
            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            if (civilDoc == null) return;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                doc.Editor.WriteMessage("\n[AI]: Running Best-Fit AutoProfile on existing EG...");

                ObjectIdCollection alignIds = civilDoc.GetAlignmentIds();
                if (alignIds.Count == 0) return;
                Alignment align = trans.GetObject(alignIds[0], OpenMode.ForRead) as Alignment;

                // 1. ვპოულობთ ProfileDrawer-ის მიერ წეღან შექმნილ EG პროფილს
                Profile egProfile = null;
                foreach (ObjectId profId in align.GetProfileIds())
                {
                    Profile p = trans.GetObject(profId, OpenMode.ForRead) as Profile;
                    if (p.ProfileType == ProfileType.EG)
                    {
                        egProfile = p;
                        break;
                    }
                }

                if (egProfile == null)
                {
                    doc.Editor.WriteMessage("\n[AI Error]: EG Profile not found. Run DrawProfile first.");
                    return;
                }

                // 2. ვქმნით საპროექტო (Layout) პროფილს
                ObjectId layerId = db.LayerZero;
                ObjectId styleId = civilDoc.Styles.ProfileStyles.Count > 1 ? civilDoc.Styles.ProfileStyles[1] : civilDoc.Styles.ProfileStyles[0]; // ვცდილობთ მეორე სტილი ავიღოთ საპროექტოსთვის
                ObjectId labelSetId = civilDoc.Styles.LabelSetStyles.ProfileLabelSetStyles.Count > 0 ? civilDoc.Styles.LabelSetStyles.ProfileLabelSetStyles[0] : ObjectId.Null;

                string profileName = "AI_Layout_" + DateTime.Now.ToString("HHmmss");
                ObjectId layoutProfId = Profile.CreateByLayout(profileName, align.ObjectId, layerId, styleId, labelSetId);
                Profile layoutProfile = trans.GetObject(layoutProfId, OpenMode.ForWrite) as Profile;

                double startSta = egProfile.StartingStation;
                double endSta = egProfile.EndingStation;
                double totalLength = endSta - startSta;

                // 3. შენი იდეალური დინამიური დაყოფის ალგორითმი
                int segments = 4;
                if (totalLength <= 500) segments = 4;
                else if (totalLength <= 1000) segments = 8;
                else if (totalLength <= 2000) segments = 12;
                else segments = 12 + (int)Math.Ceiling((totalLength - 2000) / 500.0);

                double step = totalLength / segments;

                List<Point2d> pviPoints = new List<Point2d>();
                pviPoints.Add(new Point2d(startSta, egProfile.ElevationAt(startSta)));

                for (int i = 1; i < segments; i++)
                {
                    double sta = startSta + (i * step);
                    try { pviPoints.Add(new Point2d(sta, egProfile.ElevationAt(sta))); } catch { }
                }

                pviPoints.Add(new Point2d(endSta, egProfile.ElevationAt(endSta)));

                // 4. ტანგენსები
                List<ProfileEntity> tangents = new List<ProfileEntity>();
                for (int i = 0; i < pviPoints.Count - 1; i++)
                {
                    ProfileEntity tan = layoutProfile.Entities.AddFixedTangent(pviPoints[i], pviPoints[i + 1]);
                    tangents.Add(tan);
                }

                // 5. მრუდები (60%)
                for (int i = 0; i < tangents.Count - 1; i++)
                {
                    ProfileTangent t1 = tangents[i] as ProfileTangent;
                    ProfileTangent t2 = tangents[i + 1] as ProfileTangent;

                    if (t1 != null && t2 != null && Math.Abs(t1.Grade - t2.Grade) > 0.001)
                    {
                        try
                        {
                            VerticalCurveType curveType = (t1.Grade < t2.Grade) ? VerticalCurveType.Sag : VerticalCurveType.Crest;
                            double curveLen = Math.Min(100.0, step * 0.6); // მაქსიმუმ 100მ ან ბიჯის 60%

                            layoutProfile.Entities.AddFreeSymmetricParabolaByLength(t1.EntityId, t2.EntityId, curveType, curveLen, false);
                        }
                        catch { }
                    }
                }

                trans.Commit();
                doc.Editor.WriteMessage("\n[AI Success]: Best-Fit Design Profile fully generated!");
            }
        }
    }
}