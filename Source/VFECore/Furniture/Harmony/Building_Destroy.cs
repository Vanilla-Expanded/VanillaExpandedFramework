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
			if (__instance != null && __instance.def != null && __instance.def.passability == Traversability.Impassable && __instance.Map != null)
			{
				foreach (var t in __instance.Position.GetThingList(__instance.Map).Where(b => b != __instance).ToList())
				{
					var mountableComp = t.TryGetComp<CompMountableOnWall>();
					if (mountableComp != null)
                    {
						t.Destroy(DestroyMode.Refund);
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

