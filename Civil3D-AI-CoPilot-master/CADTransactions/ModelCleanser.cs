using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;

namespace Cad_AI_Agent.CADTransactions
{
    public static class ModelCleanser
    {
        public static void Clear(Document doc)
        {
            var db = doc.Database;
            var civilDoc = CivilApplication.ActiveDocument;
            if (civilDoc == null) return;

            using var trans = db.TransactionManager.StartTransaction();
            doc.Editor.WriteMessage("\n[AI]: Sweeping the model. Deleting infrastructure objects...");

            try
            {
                // 💡 აქ გასწორდა: GetCorridorIds()-ის ნაცვლად ვიყენებთ CorridorCollection-ს
                foreach (ObjectId corrId in civilDoc.CorridorCollection)
                {
                    var obj = trans.GetObject(corrId, OpenMode.ForWrite);
                    obj.Erase();
                }

                // 2. ვშლით ღერძებს (ეს ავტომატურად წაშლის პროფილებს, ხედებს, Sample Lines-ებს და კვეთებს)
                foreach (ObjectId alignId in civilDoc.GetAlignmentIds())
                {
                    var obj = trans.GetObject(alignId, OpenMode.ForWrite);
                    obj.Erase();
                }

                trans.Commit();
                doc.Editor.Regen();
                doc.Editor.WriteMessage("\n[AI Success]: Model cleared successfully! Ready for a new prompt.");
            }
            catch (Exception ex)
            {
                doc.Editor.WriteMessage($"\n[AI Error]: Could not clear model - {ex.Message}");
            }
        }
    }
}