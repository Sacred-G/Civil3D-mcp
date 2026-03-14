using System;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.Civil.ApplicationServices;
using Autodesk.Civil.DatabaseServices;

namespace Cad_AI_Agent.CADTransactions
{
    public static class CivilLookup
    {
        public static ObjectId GetAlignmentId(CivilDocument civilDoc, Transaction trans, string alignmentName = null)
        {
            ObjectIdCollection alignmentIds = civilDoc.GetAlignmentIds();
            if (alignmentIds.Count == 0)
            {
                throw new InvalidOperationException("No alignment found in the drawing.");
            }

            if (string.IsNullOrWhiteSpace(alignmentName))
            {
                return alignmentIds[0];
            }

            foreach (ObjectId alignId in alignmentIds)
            {
                Alignment align = trans.GetObject(alignId, OpenMode.ForRead) as Alignment;
                if (align != null && string.Equals(align.Name, alignmentName, StringComparison.OrdinalIgnoreCase))
                {
                    return alignId;
                }
            }

            throw new InvalidOperationException($"Alignment '{alignmentName}' was not found.");
        }

        public static ObjectId GetSurfaceId(CivilDocument civilDoc, Transaction trans, string surfaceName = null)
        {
            ObjectIdCollection surfaceIds = civilDoc.GetSurfaceIds();
            if (surfaceIds.Count == 0)
            {
                throw new InvalidOperationException("No surface found in the drawing.");
            }

            if (string.IsNullOrWhiteSpace(surfaceName))
            {
                return surfaceIds[0];
            }

            foreach (ObjectId surfaceId in surfaceIds)
            {
                Surface surface = trans.GetObject(surfaceId, OpenMode.ForRead) as Surface;
                if (surface != null && string.Equals(surface.Name, surfaceName, StringComparison.OrdinalIgnoreCase))
                {
                    return surfaceId;
                }
            }

            throw new InvalidOperationException($"Surface '{surfaceName}' was not found.");
        }

        public static ObjectId GetProfileId(Alignment alignment, Transaction trans, string profileName = null, ProfileType? profileType = null)
        {
            ObjectIdCollection profileIds = alignment.GetProfileIds();
            if (profileIds.Count == 0)
            {
                throw new InvalidOperationException($"No profiles found on alignment '{alignment.Name}'.");
            }

            foreach (ObjectId profileId in profileIds)
            {
                Profile profile = trans.GetObject(profileId, OpenMode.ForRead) as Profile;
                if (profile == null)
                {
                    continue;
                }

                bool nameMatches = string.IsNullOrWhiteSpace(profileName) || string.Equals(profile.Name, profileName, StringComparison.OrdinalIgnoreCase);
                bool typeMatches = !profileType.HasValue || profile.ProfileType == profileType.Value;
                if (nameMatches && typeMatches)
                {
                    return profileId;
                }
            }

            if (!string.IsNullOrWhiteSpace(profileName))
            {
                throw new InvalidOperationException($"Profile '{profileName}' was not found on alignment '{alignment.Name}'.");
            }

            if (profileType.HasValue)
            {
                throw new InvalidOperationException($"No profile of type '{profileType.Value}' was found on alignment '{alignment.Name}'.");
            }

            return profileIds[0];
        }

        public static ObjectId GetAssemblyId(CivilDocument civilDoc, Transaction trans, string assemblyName = null)
        {
            var assemblyIds = civilDoc.AssemblyCollection;
            if (assemblyIds.Count == 0)
            {
                throw new InvalidOperationException("No assembly found in the drawing.");
            }

            if (string.IsNullOrWhiteSpace(assemblyName))
            {
                return assemblyIds[0];
            }

            foreach (ObjectId assemblyId in assemblyIds)
            {
                Autodesk.Civil.DatabaseServices.Assembly assembly = trans.GetObject(assemblyId, OpenMode.ForRead) as Autodesk.Civil.DatabaseServices.Assembly;
                if (assembly != null && string.Equals(assembly.Name, assemblyName, StringComparison.OrdinalIgnoreCase))
                {
                    return assemblyId;
                }
            }

            throw new InvalidOperationException($"Assembly '{assemblyName}' was not found.");
        }
    }
}
