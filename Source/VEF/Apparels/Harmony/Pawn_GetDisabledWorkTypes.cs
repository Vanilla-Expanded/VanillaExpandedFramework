using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Verse;

namespace VEF.Apparels
{
	[HarmonyPatch]
	public static class VanillaExpandedFramework_Pawn_GetDisabledWorkTypes_Patch
    {
		[HarmonyTargetMethod]
		public static MethodBase TargetMethod() => 
			AccessTools.GetDeclaredMethods(typeof(Pawn)).First(mi => mi.HasAttribute<CompilerGeneratedAttribute>() && mi.Name.Contains("GetDisabledWorkTypes"));


		[HarmonyPrefix]
		public static void Prefix(Pawn __instance, List<WorkTypeDef> list)
		{
			DisableWorkTypes(__instance.apparel?.WornApparel, list);
			DisableWorkTypes(__instance.equipment?.AllEquipmentListForReading, list);
		}

		private static void DisableWorkTypes<T>(List<T> thingList, List<WorkTypeDef> list) where T : Thing
		{
			if (thingList == null)
				return;

			foreach (var thing in thingList)
			{
				var extension = thing.def.GetModExtension<ApparelExtension>();
				if (extension != null)
				{
					if (extension.workDisables != WorkTags.None)
					{
						foreach (var allDef in DefDatabase<WorkTypeDef>.AllDefs)
						{
							if ((allDef.workTags & extension.workDisables) != 0 && !list.Contains(allDef))
							{
								list.Add(allDef);
							}
						}
					}

					if (extension.skillDisables != null)
					{
						foreach (var skill in extension.skillDisables)
						{
							foreach (var allDef in DefDatabase<WorkTypeDef>.AllDefs)
							{
								if (!list.Contains(allDef) && allDef.relevantSkills != null && allDef.relevantSkills.Contains(skill))
								{
									list.Add(allDef);
								}
							}
						}
					}
				}
			}
		}
	}
}