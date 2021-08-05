using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;
using HarmonyLib;

namespace VanillaFurnitureExpanded
{

	[HarmonyPatch(typeof(Building), "Destroy")]
	public static class Patch_BuildingDestroy
	{
		public static void Prefix(Building __instance)
		{
			Log.Message("Destroy: " + __instance);
			if (__instance != null && __instance.def != null && __instance.def.passability == Traversability.Impassable && __instance.Map != null)
			{
				foreach (var t in __instance.Position.GetThingList(__instance.Map).Where(b => b != __instance).ToList())
				{
					var mountableComp = t.TryGetComp<CompMountableOnWall>();
					if (mountableComp != null)
                    {
						t.Destroy(DestroyMode.Refund);
						Log.Message("2 Destroy: " + __instance + " - " + t);
					}
				}
			}
		}
	}

	[HarmonyPatch(typeof(GenConstruct), "BlocksConstruction")]
	public static class Patch_BlocksConstruction
	{
		public static void Postfix(Thing constructible, Thing t, ref bool __result)
		{
			if (__result)
			{
				ThingDef thingDef = ((constructible is Blueprint) ? constructible.def : ((!(constructible is Frame)) 
					? constructible.def.blueprintDef : constructible.def.entityDefToBuild.blueprintDef));
				ThingDef thingDef2 = thingDef.entityDefToBuild as ThingDef;
				if (t.def.IsSmoothed && thingDef2.HasComp(typeof(CompMountableOnWall)) && thingDef2.building != null && thingDef2.building.canPlaceOverWall)
				{
					__result = false;
				}
			}
		}
	}
}

