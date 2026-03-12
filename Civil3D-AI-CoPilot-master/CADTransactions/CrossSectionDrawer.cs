using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

namespace Cad_AI_Agent.CADTransactions
{
    public static class CrossSectionDrawer
    {
        public static void Draw(Document doc, double interval = 10.0)
        {
            var db = doc.Database;
            var civilDoc = CivilApplication.ActiveDocument;
            if (civilDoc == null) return;

            using var trans = db.TransactionManager.StartTransaction();
            doc.Editor.WriteMessage("\n[AI]: Starting Cross-Section generation...");

            if (civilDoc.GetAlignmentIds().Count == 0) return;
            var alignId = civilDoc.GetAlignmentIds()[0];

            if (trans.GetObject(alignId, OpenMode.ForRead) is not Alignment align) return;

            string slgName = "AI_SLG_" + DateTime.Now.ToString("HHmmss");
            var slgId = SampleLineGroup.Create(slgName, alignId);

            if (trans.GetObject(slgId, OpenMode.ForWrite) is not SampleLineGroup slg) return;

            // ==========================================
            // 1. სტილების ამოღება (Corridor & Surface)
            // ==========================================
            var corridorStyleId = ObjectId.Null;
            foreach (ObjectId styleId in civilDoc.Styles.CodeSetStyles)
            {
                if (trans.GetObject(styleId, OpenMode.ForRead) is Autodesk.Civil.DatabaseServices.Styles.CodeSetStyle style &&
                    style.Name.Contains("Cross_Section_Codes", StringComparison.OrdinalIgnoreCase))
                {
                    corridorStyleId = styleId;
                    break;
                }
            }
            if (corridorStyleId == ObjectId.Null && civilDoc.Styles.CodeSetStyles.Count > 0)
                corridorStyleId = civilDoc.Styles.CodeSetStyles[0];

            var surfStyleId = civilDoc.Styles.SectionStyles.Count > 0 ? civilDoc.Styles.SectionStyles[0] : ObjectId.Null;

            // ==========================================
            // 2. მოდელების ჩართვა (Sampling)
            // ==========================================
            var sources = slg.GetSectionSources();
            foreach (SectionSource source in sources)
            {
                source.IsSampled = true;
                try
                {
                    var sourceObj = trans.GetObject(source.SourceId, OpenMode.ForRead);

                    if (sourceObj is Autodesk.Civil.DatabaseServices.Corridor && corridorStyleId != ObjectId.Null)
                    {
                        source.StyleId = corridorStyleId;
                    }
                    else if (sourceObj is Autodesk.Civil.DatabaseServices.Surface && surfStyleId != ObjectId.Null)
                    {
                        source.StyleId = surfStyleId;
                    }
                }
                catch { }
            }

            // ==========================================
            // 3. Sample Line-ების დახაზვა
            // ==========================================
            double startSta = align.StartingStation;
            double endSta = align.EndingStation;
            for (double sta = startSta; sta <= endSta; sta += interval)
            {
                try
                {
                    string slName = "SL-" + Math.Round(sta, 2);
                    double x1 = 0, y1 = 0, x2 = 0, y2 = 0;
                    align.PointLocation(sta, -20.0, ref x1, ref y1);
                    align.PointLocation(sta, 20.0, ref x2, ref y2);

                    var pts = new Point2dCollection
                    {
                        new(x1, y1),
                        new(x2, y2)
                    };
                    SampleLine.Create(slName, slgId, pts);
                }
                catch { }
            }

            // ==========================================
            // 4. Band Set N1-ის მოძებნა
            // ==========================================
            var bandSetId = ObjectId.Null;
            if (civilDoc.Styles.SectionViewBandSetStyles.Count > 0)
            {
                foreach (ObjectId bId in civilDoc.Styles.SectionViewBandSetStyles)
                {
                    if (trans.GetObject(bId, OpenMode.ForRead) is Autodesk.Civil.DatabaseServices.Styles.SectionViewBandSetStyle bSet &&
                        bSet.Name.Equals("Band Set N1", StringComparison.OrdinalIgnoreCase))
                    {
                        bandSetId = bId;
                        break;
                    }
                }
                if (bandSetId == ObjectId.Null) bandSetId = civilDoc.Styles.SectionViewBandSetStyles[0];
            }

            var svStyleId = civilDoc.Styles.SectionViewStyles.Count > 0 ? civilDoc.Styles.SectionViewStyles[0] : ObjectId.Null;

            // ==========================================
            // 5. Section View-ების დახაზვა (10-კვეთიანი რიგებით)
            // ==========================================
            double startX = 0, startY = 0;
            align.PointLocation(startSta, 0, ref startX, ref startY);

            // საბაზისო წერტილი (პირველი კვეთის ადგილი)
            double baseX = startX;
            double baseY = startY - 300;

            int columns = 10;        // რამდენი კვეთი გვინდა ერთ რიგში
            double spacingX = 80.0;  // დაშორება ჰორიზონტალურად (კვეთებს შორის)
            double spacingY = 50.0;  // დაშორება ვერტიკალურად (რიგებს შორის)
            int count = 0;           // მთვლელი

            foreach (ObjectId slId in slg.GetSampleLineIds())
            {
                try
                {
                    string svName = "SV_" + DateTime.Now.ToString("HHmmssff") + "_" + count;

                    // 💡 ვითვლით მიმდინარე რიგს და სვეტს
                    int row = count / columns;
                    int col = count % columns;

                    // 💡 ვითვლით ზუსტ კოორდინატს: X იზრდება მარჯვნივ, Y მცირდება ქვევით (ახალ რიგში)
                    Point3d insertPt = new Point3d(
                        baseX + (col * spacingX),
                        baseY - (row * spacingY),
                        0
                    );

                    // 💡 SectionView.Create იღებს მკაცრად 3 პარამეტრს
                    ObjectId svId = SectionView.Create(svName, slId, insertPt);

                    // ვხსნით შექმნილ ხედს და ვადებთ მთავარ სტილს
                    SectionView sv = trans.GetObject(svId, OpenMode.ForWrite) as SectionView;
                    if (sv != null && svStyleId != ObjectId.Null)
                    {
                        sv.StyleId = svStyleId;
                    }

                    count++; // ვზრდით მთვლელს შემდეგი კვეთისთვის
                }
                catch { }
            }

            trans.Commit();
            doc.Editor.Regen();
            doc.Editor.WriteMessage("\n[AI Success]: Cross-Sections with Code Sets generated in a clean 10-column Grid!");
        }
    }
}