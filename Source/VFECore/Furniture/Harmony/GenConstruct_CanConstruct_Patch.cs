using Verse;
using RimWorld;
using HarmonyLib;
using System;

namespace VanillaFurnitureExpanded
{
    [HarmonyPatch(typeof(GenConstruct), "CanConstruct", new Type[] { typeof(Thing), typeof(Pawn), typeof(bool), typeof(bool), typeof(JobDef) })]
    public static class GenConstruct_CanConstruct_Patch
    {
        public static void Postfix(ref bool __result, Thing t)
        {
            if (__result)
            {
                try
                {
                    var thing = t?.def?.entityDefToBuild;
                    if (thing != null && thing is ThingDef def && def.HasComp(typeof(CompMountableOnWall)))
                    {
                        if (t.Position.InBounds(t.Map))
                        {
                            var edifice = t.Position.GetEdifice(t.Map);
                            if (edifice != null && edifice is not Frame && edifice.def.IsWall())
                            {
                            }
                            else
                            {
                                __result = false;
                            }
                        }
                    }
                }
                catch { }
            }
        }
    }
}

