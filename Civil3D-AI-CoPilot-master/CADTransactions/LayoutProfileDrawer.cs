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
        public static void Draw(Document doc, double[] pviData, string? alignmentName = null, string? profileName = null, double? viewOffsetY = null)
        {
            if (pviData == null || pviData.Length < 4) return;

            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            if (civilDoc == null) return;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                ObjectId alignId = CivilLookup.GetAlignmentId(civilDoc, trans, alignmentName);
                if (trans.GetObject(alignId, OpenMode.ForRead) is not Alignment align)
                {
                    doc.Editor.WriteMessage("\n[AI Error]: Alignment could not be resolved for layout profile creation.");
                    return;
                }

                ObjectId layerId = db.LayerZero;
                ObjectId styleId = civilDoc.Styles.ProfileStyles.Count > 0 ? civilDoc.Styles.ProfileStyles[0] : ObjectId.Null;
                ObjectId labelSetId = civilDoc.Styles.LabelSetStyles.ProfileLabelSetStyles.Count > 0 ? civilDoc.Styles.LabelSetStyles.ProfileLabelSetStyles[0] : ObjectId.Null;

                string resolvedProfileName = string.IsNullOrWhiteSpace(profileName)
                    ? "AI_DesignProfile_" + DateTime.Now.ToString("HHmmss")
                    : profileName;
                ObjectId profileId = Profile.CreateByLayout(resolvedProfileName, alignId, layerId, styleId, labelSetId);
                if (trans.GetObject(profileId, OpenMode.ForWrite) is not Profile layoutProfile)
                {
                    doc.Editor.WriteMessage("\n[AI Error]: Layout profile object could not be opened for editing.");
                    return;
                }

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

                double startX = 0, startY = 0;
                align.PointLocation(align.StartingStation, 0, ref startX, ref startY);

                Point3d insertPt = new Point3d(startX, startY + (viewOffsetY ?? 250.0), 0);

                ObjectId pvStyleId = civilDoc.Styles.ProfileViewStyles.Count > 0 ? civilDoc.Styles.ProfileViewStyles[0] : ObjectId.Null;
                ObjectId pvBandSetId = civilDoc.Styles.ProfileViewBandSetStyles.Count > 0 ? civilDoc.Styles.ProfileViewBandSetStyles[0] : ObjectId.Null;

                if (pvStyleId != ObjectId.Null && pvBandSetId != ObjectId.Null)
                {
                    string pvName = "AI_PV_Manual_" + DateTime.Now.ToString("HHmmss");
                    try
                    {
                        ProfileView.Create(alignId, insertPt, pvName, pvBandSetId, pvStyleId);
                    }
                    catch (Exception ex)
                    {
                        doc.Editor.WriteMessage($"\n[AI Warning]: Could not create profile view: {ex.Message}");
                    }
                }

                trans.Commit();
                doc.Editor.Regen();
            }
        }
    }
}