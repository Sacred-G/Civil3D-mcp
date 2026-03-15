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
        public static void Draw(Document doc, double interval = 150.0, string alignmentName = null, string sourceProfileName = null, string profileName = null)
        {
            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            if (civilDoc == null) return;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                doc.Editor.WriteMessage("\n[AI]: Running Best-Fit AutoProfile on existing EG...");

                ObjectId alignId = CivilLookup.GetAlignmentId(civilDoc, trans, alignmentName);
                Alignment align = trans.GetObject(alignId, OpenMode.ForRead) as Alignment;

                ObjectId sourceProfileId = string.IsNullOrWhiteSpace(sourceProfileName)
                    ? CivilLookup.GetProfileId(align, trans, null, ProfileType.EG)
                    : CivilLookup.GetProfileId(align, trans, sourceProfileName);
                Profile egProfile = trans.GetObject(sourceProfileId, OpenMode.ForRead) as Profile;

                if (egProfile == null)
                {
                    doc.Editor.WriteMessage("\n[AI Error]: EG Profile not found. Run DrawProfile first.");
                    return;
                }

                ObjectId layerId = db.LayerZero;
                ObjectId styleId = civilDoc.Styles.ProfileStyles.Count > 1 ? civilDoc.Styles.ProfileStyles[1] : civilDoc.Styles.ProfileStyles[0];
                ObjectId labelSetId = civilDoc.Styles.LabelSetStyles.ProfileLabelSetStyles.Count > 0 ? civilDoc.Styles.LabelSetStyles.ProfileLabelSetStyles[0] : ObjectId.Null;

                string resolvedProfileName = string.IsNullOrWhiteSpace(profileName)
                    ? "AI_Layout_" + DateTime.Now.ToString("HHmmss")
                    : profileName;

                ObjectId layoutProfId = Profile.CreateByLayout(resolvedProfileName, align.ObjectId, layerId, styleId, labelSetId);
                Profile layoutProfile = trans.GetObject(layoutProfId, OpenMode.ForWrite) as Profile;

                double startSta = egProfile.StartingStation;
                double endSta = egProfile.EndingStation;
                double totalLength = endSta - startSta;

                double sampleInterval = interval > 0 ? interval : 150.0;
                int segments = Math.Max(2, (int)Math.Ceiling(totalLength / sampleInterval));
                double step = totalLength / segments;

                List<Point2d> pviPoints = new List<Point2d>();
                pviPoints.Add(new Point2d(startSta, egProfile.ElevationAt(startSta)));

                for (int i = 1; i < segments; i++)
                {
                    double sta = startSta + (i * step);
                    try { pviPoints.Add(new Point2d(sta, egProfile.ElevationAt(sta))); }
                    catch (Exception ex) { doc.Editor.WriteMessage($"\n[AI Warning]: Could not sample elevation at station {sta:F2}: {ex.Message}"); }
                }

                pviPoints.Add(new Point2d(endSta, egProfile.ElevationAt(endSta)));

                List<ProfileEntity> tangents = new List<ProfileEntity>();
                for (int i = 0; i < pviPoints.Count - 1; i++)
                {
                    ProfileEntity tan = layoutProfile.Entities.AddFixedTangent(pviPoints[i], pviPoints[i + 1]);
                    tangents.Add(tan);
                }

                for (int i = 0; i < tangents.Count - 1; i++)
                {
                    ProfileTangent t1 = tangents[i] as ProfileTangent;
                    ProfileTangent t2 = tangents[i + 1] as ProfileTangent;

                    if (t1 != null && t2 != null && Math.Abs(t1.Grade - t2.Grade) > 0.001)
                    {
                        try
                        {
                            VerticalCurveType curveType = (t1.Grade < t2.Grade) ? VerticalCurveType.Sag : VerticalCurveType.Crest;
                            double curveLen = Math.Min(100.0, step * 0.6);

                            layoutProfile.Entities.AddFreeSymmetricParabolaByLength(t1.EntityId, t2.EntityId, curveType, curveLen, false);
                        }
                        catch (Exception ex)
                        {
                            doc.Editor.WriteMessage($"\n[AI Warning]: Could not add vertical curve between tangents {i} and {i+1}: {ex.Message}");
                        }
                    }
                }

                trans.Commit();
                doc.Editor.WriteMessage("\n[AI Success]: Best-Fit Design Profile fully generated!");
            }
        }
    }
}