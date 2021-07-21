using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VanillaApparelExpanded
{
	[HarmonyPatch(typeof(SkillRecord), "Learn")]
	public static class SkillRecord_Learn_Patch
	{
		public static void Prefix(SkillRecord __instance, Pawn ___pawn, ref float xp, bool direct = false)
		{
			if (___pawn.apparel?.WornApparel != null)
            {
				foreach (var apparel in ___pawn.apparel.WornApparel)
                {
					var extension = apparel.def.GetModExtension<ApparelExtension>();
					if (extension != null && extension.skillGainModifier.HasValue)
                    {
						xp *= extension.skillGainModifier.Value;
					}
                }
			}
		}
	}
}