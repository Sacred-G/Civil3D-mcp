using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using CoreApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Cad_AI_Agent.CADTransactions
{
    public class CogoPointDrawer
    {
        public static void Draw(Document doc, double x, double y, double elevation, string description = "AI_Point")
        {
            Database db = doc.Database;

            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            if (civilDoc == null) return;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                CogoPointCollection cogoPoints = civilDoc.CogoPoints;

                Point3d location = new Point3d(x, y, elevation);

                ObjectId pointId = cogoPoints.Add(location, description, true);

                trans.Commit();
            }
        }
    }
}