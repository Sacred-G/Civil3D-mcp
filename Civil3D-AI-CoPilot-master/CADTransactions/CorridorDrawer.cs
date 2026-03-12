using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

namespace Cad_AI_Agent.CADTransactions
{
    public static class CorridorDrawer
    {
        public static void Draw(Document doc, string baseName = "AI_Corridor")
        {
            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            if (civilDoc == null) return;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                if (civilDoc.GetAlignmentIds().Count == 0) return;
                ObjectId alignId = civilDoc.GetAlignmentIds()[0];
                Alignment align = trans.GetObject(alignId, OpenMode.ForRead) as Alignment;

                ObjectId layoutProfId = ObjectId.Null;
                foreach (ObjectId profId in align.GetProfileIds())
                {
                    Profile p = trans.GetObject(profId, OpenMode.ForRead) as Profile;
                    if (p.ProfileType == ProfileType.FG)
                    {
                        layoutProfId = profId;
                        break;
                    }
                }
                if (layoutProfId == ObjectId.Null) return;

                var assemblyIds = civilDoc.AssemblyCollection;
                if (assemblyIds.Count == 0) return;
                ObjectId assemblyId = assemblyIds[0];

                string corrName = baseName + "_" + DateTime.Now.ToString("HHmmss");
                ObjectId corrId = civilDoc.CorridorCollection.Add(corrName, "AI_Baseline", alignId, layoutProfId, "AI_Region", assemblyId);

                Corridor corridor = trans.GetObject(corrId, OpenMode.ForWrite) as Corridor;
                corridor.Rebuild(); // პირველი ინიციალიზაცია

                if (corridor.Baselines.Count > 0 && corridor.Baselines[0].BaselineRegions.Count > 0)
                {
                    BaselineRegion region = corridor.Baselines[0].BaselineRegions[0];

                    // 1. სიხშირე 10მ
                    for (double s = region.StartStation; s <= region.EndStation; s += 10.0)
                    {
                        try { region.AddStation(s, "AI_10m"); } catch { }
                    }

                    // 2. ზედაპირის სამიზნეები (Target Override ტრიუკი)
                    ObjectIdCollection surfIds = civilDoc.GetSurfaceIds();
                    if (surfIds.Count > 0)
                    {
                        ObjectId surfId = surfIds[0];
                        SubassemblyTargetInfoCollection targets = region.GetTargets();
                        bool targetUpdated = false;

                        foreach (SubassemblyTargetInfo target in targets)
                        {
                            if (target.TargetType == SubassemblyLogicalNameType.Surface)
                            {
                                // ვქმნით ახალ კოლექციას და ვაწერთ ზედ!
                                ObjectIdCollection newTargetIds = new ObjectIdCollection();
                                newTargetIds.Add(surfId);
                                target.TargetIds = newTargetIds;
                                targetUpdated = true;
                            }
                        }

                        if (targetUpdated)
                        {
                            region.SetTargets(targets);
                        }
                    }
                }

                corridor.Rebuild(); // მეორე Rebuild სამიზნეებისთვის
                trans.Commit();
            }
        }
    }
}