using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace VFECore
{


    internal class Patch_Pawn_ApparelTracker
	{
		[HarmonyPatch(typeof(Pawn_ApparelTracker), "TryDrop")]
		[HarmonyPatch(new Type[]
		{
			typeof(Apparel),
			typeof(Apparel),
			typeof(IntVec3),
			typeof(bool)
		}, new ArgumentType[]
		{
			0,
			ArgumentType.Ref,
			0,
			0
		})]
		public class TryDrop_Patch
		{
			public static void Postfix(Pawn ___pawn, bool __result, Apparel ap, ref Apparel resultingAp, IntVec3 pos, bool forbid = true)
			{
				if (__result && ___pawn != null && ap is Apparel_Shield newShield)
				{
					newShield.CompShield.equippedOffHand = false;
					var comp = newShield.GetComp<CompEquippable>();
					if (comp != null)
                    {
						foreach (var verb in comp.AllVerbs)
						{
							verb.caster = null;
							verb.Reset();
						}
					}
				}
			}
		}

		[HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Wear))]
		public static class Wear_Patch
		{
			public static void Postfix(Apparel newApparel, bool dropReplacedApparel = true, bool locked = false)
			{
				if (newApparel is Apparel_Shield newShield)
				{
					newShield.CompShield.equippedOffHand = true;
					var comp = newShield.GetComp<CompEquippable>();
					if (comp != null)
					{
						foreach (var verb in comp.AllVerbs)
						{
							verb.caster = newShield.Wearer;
							verb.Reset();
						}
					}
				}
			}
		}
	}
}
