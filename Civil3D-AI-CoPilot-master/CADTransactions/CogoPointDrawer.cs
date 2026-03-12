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

            // ვიღებთ Civil 3D-ის აქტიურ დოკუმენტს
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            if (civilDoc == null) return;

            using (Transaction trans = db.TransactionManager.StartTransaction())
            {
                // ვიღებთ Cogo Point-ების კოლექციას ამ ნახაზიდან
                CogoPointCollection cogoPoints = civilDoc.CogoPoints;

                // ვქმნით წერტილს (X, Y და Z ნიშნული)
                Point3d location = new Point3d(x, y, elevation);

                // ვამატებთ წერტილს ნახაზში
                ObjectId pointId = cogoPoints.Add(location, description, true);

                trans.Commit();
            }
        }
    }
}