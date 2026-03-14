using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

namespace Cad_AI_Agent.CADTransactions
{
    public static class CorridorDrawer
    {
        public static void Draw(Document doc, string corridorName = null, string alignmentName = null, string profileName = null, string assemblyName = null, string surfaceName = null, double frequency = 10.0)
        {
            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            if (civilDoc == null) return;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                ObjectId alignId = CivilLookup.GetAlignmentId(civilDoc, trans, alignmentName);
                Alignment align = trans.GetObject(alignId, OpenMode.ForRead) as Alignment;

                ObjectId layoutProfId = string.IsNullOrWhiteSpace(profileName)
                    ? CivilLookup.GetProfileId(align, trans, null, ProfileType.FG)
                    : CivilLookup.GetProfileId(align, trans, profileName);

                ObjectId assemblyId = CivilLookup.GetAssemblyId(civilDoc, trans, assemblyName);

                string corrName = string.IsNullOrWhiteSpace(corridorName)
                    ? "AI_Corridor_" + DateTime.Now.ToString("HHmmss")
                    : corridorName;
                ObjectId corrId = civilDoc.CorridorCollection.Add(corrName, "AI_Baseline", alignId, layoutProfId, "AI_Region", assemblyId);

                Corridor corridor = trans.GetObject(corrId, OpenMode.ForWrite) as Corridor;
                corridor.Rebuild(); // პირველი ინიციალიზაცია

                if (corridor.Baselines.Count > 0 && corridor.Baselines[0].BaselineRegions.Count > 0)
                {
                    BaselineRegion region = corridor.Baselines[0].BaselineRegions[0];

                    // 1. სიხშირე 10მ
                    double resolvedFrequency = frequency > 0 ? frequency : 10.0;
                    for (double s = region.StartStation; s <= region.EndStation; s += resolvedFrequency)
                    {
                        try { region.AddStation(s, "AI_10m"); } catch { }
                    }

                    if (civilDoc.GetSurfaceIds().Count > 0 || !string.IsNullOrWhiteSpace(surfaceName))
                    {
                        ObjectId surfId = CivilLookup.GetSurfaceId(civilDoc, trans, surfaceName);
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