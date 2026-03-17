using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

namespace Cad_AI_Agent.CADTransactions
{
    public static class CorridorDrawer
    {
        public static void Draw(Document doc, string? corridorName = null, string? alignmentName = null, string? profileName = null, string? assemblyName = null, string? surfaceName = null, double frequency = 10.0)
        {
            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            if (civilDoc == null) return;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                ObjectId alignId = CivilLookup.GetAlignmentId(civilDoc, trans, alignmentName);
                if (trans.GetObject(alignId, OpenMode.ForRead) is not Alignment align)
                {
                    doc.Editor.WriteMessage("\n[AI Error]: Alignment could not be resolved for corridor creation.");
                    return;
                }

                ObjectId layoutProfId = string.IsNullOrWhiteSpace(profileName)
                    ? CivilLookup.GetProfileId(align, trans, null, ProfileType.FG)
                    : CivilLookup.GetProfileId(align, trans, profileName);

                ObjectId assemblyId = CivilLookup.GetAssemblyId(civilDoc, trans, assemblyName);

                string corrName = string.IsNullOrWhiteSpace(corridorName)
                    ? "AI_Corridor_" + DateTime.Now.ToString("HHmmss")
                    : corridorName;
                ObjectId corrId = civilDoc.CorridorCollection.Add(corrName, "AI_Baseline", alignId, layoutProfId, "AI_Region", assemblyId);

                if (trans.GetObject(corrId, OpenMode.ForWrite) is not Corridor corridor)
                {
                    doc.Editor.WriteMessage("\n[AI Error]: Corridor object could not be created.");
                    return;
                }
                corridor.Rebuild(); 

                if (corridor.Baselines.Count > 0 && corridor.Baselines[0].BaselineRegions.Count > 0)
                {
                    BaselineRegion region = corridor.Baselines[0].BaselineRegions[0];

                    double resolvedFrequency = frequency > 0 ? frequency : 10.0;
                    for (double s = region.StartStation; s <= region.EndStation; s += resolvedFrequency)
                    {
                        try { region.AddStation(s, "AI_10m"); }
                        catch (Exception ex) { doc.Editor.WriteMessage($"\n[AI Warning]: Could not add station at {s:F2}: {ex.Message}"); }

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

                corridor.Rebuild(); 
                trans.Commit();
            }
        }
    }
}