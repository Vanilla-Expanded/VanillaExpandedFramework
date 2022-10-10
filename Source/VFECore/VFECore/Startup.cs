using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;

namespace VFECore
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        public static Mesh plane20Flip = MeshMakerPlanes.NewPlaneMesh(2f, true);

        static Startup()
        {
            // Cache setters
            PawnShieldGenerator.Reset();
            ScenPartUtility.SetCache();
            CheckXmlErrors();
        }

        public static void CheckXmlErrors()
        {
            foreach (var def in DefDatabase<WorkGiverDef>.AllDefs)
            {
                if (def.giverClass is null)
                {
                    Log.Error(def.defName + " is missing worker class and will not work properly. Report about it to " + def.modContentPack?.Name + " devs.");
                }
            }
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def.thingClass is null)
                {
                    Log.Error(def.defName + " is missing thing class and will not work properly. Report about it to " + def.modContentPack?.Name + " devs.");
                }
                if (def.comps != null)
                {
                    foreach (var compProps in def.comps)
                    {
                        if (compProps.compClass is null)
                        {
                            Log.Error(def.defName + " one of comps is missing comp class and will not work properly. Report about it to " + def.modContentPack?.Name + " devs.");
                        }
                    }
                }
            }

            foreach (var def in DefDatabase<SoundDef>.AllDefs)
            {
                foreach (var subSound in def.subSounds)
                {
                    foreach (var grain in subSound.grains)
                    {
                        if (grain.GetResolvedGrains().Any() is false)
                        {
                            Log.Error(def.defName + " sound is missing resolved grains and will not work properly. Report about it to " + def.modContentPack?.Name + " devs.");
                        }
                    }
                }
            }
        }

    }
}