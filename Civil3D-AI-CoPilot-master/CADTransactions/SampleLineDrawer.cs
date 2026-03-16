using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

namespace Cad_AI_Agent.CADTransactions
{
    public static class SampleLineDrawer
    {
        public static void Draw(Document doc, double leftWidth, double rightWidth, double interval, string? alignmentName = null, string? groupName = null)
        {
            if (interval <= 0 || leftWidth <= 0 || rightWidth <= 0)
            {
                doc.Editor.WriteMessage("\n[AI Error]: Sample line widths and interval must be greater than zero.");
                return;
            }

            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            if (civilDoc == null) return;

            using Transaction trans = db.TransactionManager.StartTransaction();

            ObjectId alignId = CivilLookup.GetAlignmentId(civilDoc, trans, alignmentName);
            if (trans.GetObject(alignId, OpenMode.ForRead) is not Alignment align) return;

            string resolvedGroupName = string.IsNullOrWhiteSpace(groupName)
                ? "AI_SLG_" + DateTime.Now.ToString("HHmmss")
                : groupName;
            ObjectId groupId = SampleLineGroup.Create(resolvedGroupName, align.ObjectId);
            if (trans.GetObject(groupId, OpenMode.ForWrite) is not SampleLineGroup group) return;

            SectionSourceCollection sources = group.GetSectionSources();
            foreach (SectionSource source in sources)
            {
                source.IsSampled = true;
            }

            int created = 0;
            for (double station = align.StartingStation; station <= align.EndingStation; station += interval)
            {
                try
                {
                    double x1 = 0;
                    double y1 = 0;
                    double x2 = 0;
                    double y2 = 0;
                    align.PointLocation(station, -leftWidth, ref x1, ref y1);
                    align.PointLocation(station, rightWidth, ref x2, ref y2);

                    Point2dCollection points = new Point2dCollection
                    {
                        new Point2d(x1, y1),
                        new Point2d(x2, y2)
                    };

                    SampleLine.Create($"SL-{station:0.##}", groupId, points);
                    created++;
                }
                catch
                {
                }
            }

            trans.Commit();
            doc.Editor.Regen();
            doc.Editor.WriteMessage($"\n[AI Success]: {created} sample lines created in group '{resolvedGroupName}'.");
        }
    }
}
