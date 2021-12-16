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
				if (__result && ___pawn != null)
				{
					if (resultingAp is Apparel_Shield newShield)
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
					var extension = resultingAp?.def.GetModExtension<ApparelExtension>();
					if (extension != null)
                    {
						if (___pawn.story?.traits != null)
                        {
							if (extension.traitsOnUnequip != null)
                            {
								foreach (var traitDef in extension.traitsOnUnequip)
								{
									var trait = ___pawn.story.traits.GetTrait(traitDef);
									if (trait != null)
									{
										___pawn.story.traits.RemoveTrait(trait);
									}
								}
							}
						}

                    }
				}
			}
		}

		[HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Remove))]
		public class Pawn_ApparelTracker_Remove_Patch
		{
			public static void Postfix(Pawn ___pawn, Apparel ap)
			{
				if (___pawn != null)
				{
					var extension = ap?.def.GetModExtension<ApparelExtension>();
					if (extension != null)
					{
						if (___pawn.story?.traits != null)
						{
							if (extension.traitsOnUnequip != null)
							{
								foreach (var traitDef in extension.traitsOnUnequip)
								{
									var trait = ___pawn.story.traits.GetTrait(traitDef);
									if (trait != null)
									{
										___pawn.story.traits.RemoveTrait(trait);
									}
								}
							}
						}
		
					}
				}
			}
		}

		[HarmonyPatch(typeof(Pawn_ApparelTracker), nameof(Pawn_ApparelTracker.Wear))]
		public static class Wear_Patch
		{
			public static void Postfix(Pawn_ApparelTracker __instance, Apparel newApparel, bool dropReplacedApparel = true, bool locked = false)
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

				var extension = newApparel?.def.GetModExtension<ApparelExtension>();
				if (extension != null)
				{
					if (__instance.pawn.story?.traits != null)
					{
						if (extension.traitsOnEquip != null)
						{
							foreach (var traitDef in extension.traitsOnEquip)
							{
								if (!__instance.pawn.story.traits.HasTrait(traitDef))
								{
									__instance.pawn.story.traits.GainTrait(new Trait(traitDef));
								}
							}
						}
					}
				}
			}
		}
	}
}
