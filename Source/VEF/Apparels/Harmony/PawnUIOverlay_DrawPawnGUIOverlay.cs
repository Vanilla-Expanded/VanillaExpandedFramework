using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VEF.Apparels
{
	[HarmonyPatch(typeof(PawnUIOverlay), "DrawPawnGUIOverlay")]
	public static class VanillaExpandedFramework_PawnUIOverlay_Patch
    {
		[HarmonyPrefix]
		public static bool GhillieException(PawnUIOverlay __instance, Pawn ___pawn)
		{
			bool flag;
			if (___pawn.Faction.HostileTo(Faction.OfPlayer) && ___pawn.apparel != null && ___pawn.apparel.WornApparel != null)
			{
				flag = StaticCollectionsClass.camouflaged_pawns.Contains(___pawn);
			}
			else
			{
				flag = false;
			}
			return !flag;
		}
	}
}