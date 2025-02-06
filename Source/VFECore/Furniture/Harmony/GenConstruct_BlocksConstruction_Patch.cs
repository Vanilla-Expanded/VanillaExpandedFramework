using Verse;
using RimWorld;
using HarmonyLib;

namespace VanillaFurnitureExpanded
{
    [HarmonyPatch(typeof(GenConstruct), "BlocksConstruction")]
	public static class GenConstruct_BlocksConstruction_Patch
    {
		public static void Postfix(Thing constructible, Thing t, ref bool __result)
		{
			if (__result)
			{
				try
                {
                    ThingDef thingDef = ((constructible is Blueprint) ? constructible.def : ((!(constructible is Frame))
                            ? constructible.def.blueprintDef : constructible.def.entityDefToBuild.blueprintDef));
                    ThingDef thingDef2 = thingDef.entityDefToBuild as ThingDef;
                    bool isMountableOnWall = thingDef2?.building != null && thingDef2.building.canPlaceOverWall
                        && thingDef2.HasComp(typeof(CompMountableOnWall));
                    if (isMountableOnWall && t.def.IsWall())
                    {
                        __result = false;
                    }
                }
                catch { }
			}
		}

        public static bool IsWall(this ThingDef def)
        {
            if (def.IsEdifice())
            {
                return def.IsSmoothed || def.defName.ToLower().Contains("wall") 
                    || (def.graphicData?.linkFlags.HasFlag(LinkFlags.Wall) ?? false) 
                    || def.building != null && def.building.supportsWallAttachments;
            }
            return false;
        }
    }
}

