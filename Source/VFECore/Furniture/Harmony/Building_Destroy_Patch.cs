using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;
using HarmonyLib;

namespace VanillaFurnitureExpanded
{

    [HarmonyPatch(typeof(Building), "Destroy")]
	public static class Building_Destroy_Patch
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
}

