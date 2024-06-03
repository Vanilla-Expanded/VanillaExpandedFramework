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

					if (thingDef2?.building != null && thingDef2.building.canPlaceOverWall && thingDef2.HasComp(typeof(CompMountableOnWall)) &&
						(t.def.IsSmoothed || t.def.defName.ToLower().Contains("wall")))
					{
						__result = false;
					}
				}
				catch { }
			}
		}
	}
}

