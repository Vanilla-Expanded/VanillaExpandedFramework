using System;
using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaApparelExpanded
{
	using System.Linq;
	using System.Reflection;
	using System.Runtime.CompilerServices;

	[HarmonyPatch]
	public static class Pawn_GetDisabledWorkTypes_Patch
	{
		[HarmonyTargetMethod]
		public static MethodBase TargetMethod() => 
			AccessTools.GetDeclaredMethods(typeof(Pawn)).First(mi => mi.HasAttribute<CompilerGeneratedAttribute>() && mi.Name.Contains("GetDisabledWorkTypes"));


		[HarmonyPrefix]
		public static void Prefix(Pawn __instance, List<WorkTypeDef> list)
		{
			if (__instance.apparel?.WornApparel != null)
			{
				foreach (var apparel in __instance.apparel.WornApparel)
				{
					var extension = apparel.def.GetModExtension<ApparelExtension>();
					if (extension != null)
					{
						if (extension.workDisables != null)
						{
							foreach (var workTag in extension.workDisables)
							{
								foreach (WorkTypeDef allDef in DefDatabase<WorkTypeDef>.AllDefs)
								{
									if (!list.Contains(allDef) && (allDef.workTags & workTag) != 0)
									{
										list.Add(allDef);
									}
								}
							}
						}

						if (extension.skillDisables != null)
						{
							foreach (var skill in extension.skillDisables)
							{
								foreach (WorkTypeDef allDef in DefDatabase<WorkTypeDef>.AllDefs)
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
}