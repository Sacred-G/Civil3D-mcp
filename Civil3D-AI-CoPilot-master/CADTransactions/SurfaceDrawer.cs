using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;
using CivilSurface = Autodesk.Civil.DatabaseServices.Surface;

namespace Cad_AI_Agent.CADTransactions
{
    public static class SurfaceDrawer
    {
        public static void Draw(Document doc, double[] pointData, string? surfaceName = null)
        {
            if (pointData == null || pointData.Length < 9 || pointData.Length % 3 != 0)
            {
                doc.Editor.WriteMessage("\n[AI Error]: Surface creation requires at least three XYZ points.");
                return;
            }

            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            if (civilDoc == null) return;

            using Transaction trans = db.TransactionManager.StartTransaction();

            ObjectId styleId = civilDoc.Styles.SurfaceStyles.Count > 0 ? civilDoc.Styles.SurfaceStyles[0] : ObjectId.Null;
            string resolvedSurfaceName = string.IsNullOrWhiteSpace(surfaceName)
                ? "AI_Surface_" + DateTime.Now.ToString("HHmmss")
                : surfaceName;
            ObjectId surfaceId = CreateTinSurface(civilDoc, resolvedSurfaceName, styleId);
            if (trans.GetObject(surfaceId, OpenMode.ForWrite) is not CivilSurface surface) return;

            object definition = GetSurfaceDefinition(surface);
            Point3dCollection points = ToPoint3dCollection(pointData);
            bool added = TryInvokeMethod(definition, "AddPoints", points)
                || TryInvokeMethod(definition, "AddVertices", points);

            if (!added)
            {
                throw new InvalidOperationException($"Unable to add points to surface '{resolvedSurfaceName}'.");
            }

            InvokeMethod(surface, "Rebuild");
            trans.Commit();
            doc.Editor.Regen();
            doc.Editor.WriteMessage($"\n[AI Success]: TIN surface '{resolvedSurfaceName}' created from {points.Count} points.");
        }

        public static void AddBreakline(Document doc, double[] pointData, string? surfaceName = null)
        {
            if (pointData == null || pointData.Length < 6 || pointData.Length % 3 != 0)
            {
                doc.Editor.WriteMessage("\n[AI Error]: Breakline creation requires at least two XYZ points.");
                return;
            }

            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            if (civilDoc == null) return;

            using Transaction trans = db.TransactionManager.StartTransaction();

            ObjectId surfaceId = CivilLookup.GetSurfaceId(civilDoc, trans, surfaceName);
            if (trans.GetObject(surfaceId, OpenMode.ForWrite) is not CivilSurface surface) return;

            object definition = GetSurfaceDefinition(surface);
            Point3dCollection points = ToPoint3dCollection(pointData);
            bool added = TryInvokeMethod(definition, "AddBreaklines", points, 1.0, 0.0, 0.0, 0.0)
                || TryInvokeMethod(definition, "AddStandardBreaklines", points, 1.0, 0.0, 0.0, 0.0);

            if (!added)
            {
                throw new InvalidOperationException($"Unable to add a breakline to surface '{surface.Name}'.");
            }

            InvokeMethod(surface, "Rebuild");
            trans.Commit();
            doc.Editor.Regen();
            doc.Editor.WriteMessage($"\n[AI Success]: Breakline added to surface '{surface.Name}'.");
        }

        public static void ExtractContours(Document doc, double minorInterval, double majorInterval, string? surfaceName = null)
        {
            if (minorInterval <= 0 || majorInterval <= 0)
            {
                doc.Editor.WriteMessage("\n[AI Error]: Contour intervals must be greater than zero.");
                return;
            }

            Database db = doc.Database;
            CivilDocument civilDoc = CivilApplication.ActiveDocument;
            if (civilDoc == null) return;

            using Transaction trans = db.TransactionManager.StartTransaction();

            ObjectId surfaceId = CivilLookup.GetSurfaceId(civilDoc, trans, surfaceName);
            if (trans.GetObject(surfaceId, OpenMode.ForRead) is not CivilSurface surface) return;

            List<ObjectId> extracted = ExtractContourEntities(surface, minorInterval, majorInterval);
            trans.Commit();
            doc.Editor.Regen();
            doc.Editor.WriteMessage($"\n[AI Success]: {extracted.Count} contour entities extracted from surface '{surface.Name}'.");
        }

        private static Point3dCollection ToPoint3dCollection(double[] pointData)
        {
            Point3dCollection points = new Point3dCollection();
            for (int i = 0; i + 2 < pointData.Length; i += 3)
            {
                points.Add(new Point3d(pointData[i], pointData[i + 1], pointData[i + 2]));
            }

            return points;
        }

        private static ObjectId CreateTinSurface(CivilDocument civilDoc, string name, ObjectId styleId)
        {
            List<MethodInfo> methods = typeof(TinSurface)
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(m => m.Name == "Create")
                .OrderByDescending(m => m.GetParameters().Length)
                .ToList();

            foreach (MethodInfo method in methods)
            {
                ParameterInfo[] parameters = method.GetParameters();
                try
                {
                    if (parameters.Length == 3 && parameters[0].ParameterType.Name == "CivilDocument")
                    {
                        object? created = method.Invoke(null, new object[] { civilDoc, name, styleId });
                        if (created is ObjectId objectId)
                        {
                            return objectId;
                        }
                    }

                    if (parameters.Length == 2 && parameters[0].ParameterType == typeof(string) && parameters[1].ParameterType == typeof(ObjectId))
                    {
                        object? created = method.Invoke(null, new object[] { name, styleId });
                        if (created is ObjectId objectId)
                        {
                            return objectId;
                        }
                    }

                    if (parameters.Length == 2 && parameters[0].ParameterType.Name == "CivilDocument")
                    {
                        object? created = method.Invoke(null, new object[] { civilDoc, name });
                        if (created is ObjectId objectId)
                        {
                            return objectId;
                        }
                    }

                    if (parameters.Length == 1 && parameters[0].ParameterType == typeof(string))
                    {
                        object? created = method.Invoke(null, new object[] { name });
                        if (created is ObjectId objectId)
                        {
                            return objectId;
                        }
                    }
                }
                catch
                {
                }
            }

            throw new InvalidOperationException("Unable to create a TIN surface with the available Civil 3D API overloads.");
        }

        private static object GetSurfaceDefinition(CivilSurface surface)
        {
            foreach (string propertyName in new[] { "Definition", "Operations", "BoundariesDefinition" })
            {
                object? value = GetPropertyValue(surface, propertyName);
                if (value != null)
                {
                    return value;
                }
            }

            throw new InvalidOperationException($"Surface definition API is not available for surface '{surface.Name}'.");
        }

        private static List<ObjectId> ExtractContourEntities(CivilSurface surface, double minorInterval, double majorInterval)
        {
            foreach (string methodName in new[] { "ExtractContours", "ExtractContour" })
            {
                object? value = InvokeMethod(surface, methodName, minorInterval, majorInterval);
                if (value == null)
                {
                    continue;
                }
                List<ObjectId> objectIds = ToObjectIds(value).ToList();
                if (objectIds.Count > 0)
                {
                    return objectIds;
                }
            }

            throw new InvalidOperationException($"Unable to extract contours for surface '{surface.Name}'.");
        }

        private static IEnumerable<ObjectId> ToObjectIds(object? collection)
        {
            if (collection is ObjectIdCollection objectIds)
            {
                foreach (ObjectId objectId in objectIds)
                {
                    yield return objectId;
                }

                yield break;
            }

            if (collection is IEnumerable enumerable)
            {
                foreach (object item in enumerable)
                {
                    if (item is ObjectId objectId)
                    {
                        yield return objectId;
                    }
                }
            }
        }

        private static object? InvokeMethod(object? target, string methodName, params object[] arguments)
        {
            if (target == null)
            {
                return null;
            }

            MethodInfo[] methods = target.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.Name == methodName)
                .ToArray();

            foreach (MethodInfo method in methods)
            {
                ParameterInfo[] parameters = method.GetParameters();
                if (parameters.Length != arguments.Length)
                {
                    continue;
                }

                try
                {
                    return method.Invoke(target, arguments);
                }
                catch
                {
                }
            }

            return null;
        }

        private static object? GetPropertyValue(object? target, string propertyName)
        {
            if (target == null)
            {
                return null;
            }

            PropertyInfo? property = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            return property?.GetValue(target);
        }

        private static bool TryInvokeMethod(object target, string methodName, params object[] preferredArguments)
        {
            IEnumerable<MethodInfo> methods = target.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(method => method.Name == methodName)
                .OrderByDescending(method => method.GetParameters().Length);

            foreach (MethodInfo method in methods)
            {
                object[]? arguments = BuildInvocationArguments(method.GetParameters(), preferredArguments);
                if (arguments == null)
                {
                    continue;
                }

                try
                {
                    method.Invoke(target, arguments);
                    return true;
                }
                catch
                {
                }
            }

            return false;
        }

        private static object[]? BuildInvocationArguments(ParameterInfo[] parameters, object[] preferredArguments)
        {
            List<object> remaining = new List<object>(preferredArguments);
            object[] result = new object[parameters.Length];

            for (int index = 0; index < parameters.Length; index++)
            {
                ParameterInfo parameter = parameters[index];
                int matchIndex = remaining.FindIndex(candidate => IsCompatibleArgument(parameter.ParameterType, candidate));
                if (matchIndex >= 0)
                {
                    result[index] = remaining[matchIndex];
                    remaining.RemoveAt(matchIndex);
                    continue;
                }

                if (TryCreateDefaultArgument(parameter.ParameterType, out object? defaultValue))
                {
                    result[index] = defaultValue!;
                    continue;
                }

                return null;
            }

            return result;
        }

        private static bool IsCompatibleArgument(Type parameterType, object? candidate)
        {
            if (candidate == null)
            {
                return !parameterType.IsValueType || Nullable.GetUnderlyingType(parameterType) != null;
            }

            if (parameterType.IsInstanceOfType(candidate))
            {
                return true;
            }

            Type underlyingType = Nullable.GetUnderlyingType(parameterType) ?? parameterType;
            return underlyingType == typeof(double) && candidate is double
                || underlyingType == typeof(bool) && candidate is bool
                || underlyingType == typeof(int) && candidate is int;
        }

        private static bool TryCreateDefaultArgument(Type parameterType, out object? value)
        {
            Type underlyingType = Nullable.GetUnderlyingType(parameterType) ?? parameterType;

            if (underlyingType == typeof(double))
            {
                value = 0d;
                return true;
            }

            if (underlyingType == typeof(int))
            {
                value = 0;
                return true;
            }

            if (underlyingType == typeof(bool))
            {
                value = false;
                return true;
            }

            if (underlyingType.IsEnum)
            {
                value = Enum.GetValues(underlyingType).GetValue(0);
                return true;
            }

            value = null;
            return !parameterType.IsValueType || Nullable.GetUnderlyingType(parameterType) != null;
        }
    }
}
