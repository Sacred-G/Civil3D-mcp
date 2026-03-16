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
        public static void Draw(Document doc, double offsetX, double offsetY, string? alignmentName = null, string? surfaceName = null, string? profileName = null)
        {
            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            if (civilDoc == null) return;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                doc.Editor.WriteMessage("\n[AI]: Drawing Profile View relative to Alignment start...");

                ObjectId alignId = CivilLookup.GetAlignmentId(civilDoc, trans, alignmentName);
                if (trans.GetObject(alignId, OpenMode.ForRead) is not Alignment align)
                {
                    doc.Editor.WriteMessage("\n[AI Error]: Alignment could not be resolved for profile creation.");
                    return;
                }
                ObjectId surfId = CivilLookup.GetSurfaceId(civilDoc, trans, surfaceName);

                ObjectId layerId = db.LayerZero;
                ObjectId styleId = civilDoc.Styles.ProfileStyles.Count > 0 ? civilDoc.Styles.ProfileStyles[0] : ObjectId.Null;
                ObjectId labelSetId = civilDoc.Styles.LabelSetStyles.ProfileLabelSetStyles.Count > 0 ? civilDoc.Styles.LabelSetStyles.ProfileLabelSetStyles[0] : ObjectId.Null;

                string resolvedProfileName = string.IsNullOrWhiteSpace(profileName)
                    ? "AI_SurfaceProfile_" + DateTime.Now.ToString("HHmmss")
                    : profileName;
                Profile.CreateFromSurface(resolvedProfileName, alignId, surfId, layerId, styleId, labelSetId);

                double startX = 0, startY = 0;
                try
                {
                    align.PointLocation(align.StartingStation, 0, ref startX, ref startY);
                }
                catch
                {
                }

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