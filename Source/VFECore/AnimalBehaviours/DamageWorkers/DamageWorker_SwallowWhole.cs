using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse.Sound;
using Verse;
using HarmonyLib;

namespace AnimalBehaviours
{
    [HarmonyPatch(typeof(Thing), "TakeDamage")]
    public static class Patch_TakeDamage
    {
        public static Thing instigatorToSet;
        public static void Prefix(Thing __instance, DamageInfo dinfo)
        {
            if (instigatorToSet != null && dinfo.Instigator is null)
            {
                AccessTools.Field(typeof(DamageInfo), "instigatorInt").SetValueDirect(__makeref(dinfo), instigatorToSet);
            }
        }
    }

    public class DamageWorker_SwallowWhole : DamageWorker_Cut
    {
        protected override void ApplySpecialEffectsToPart(Pawn pawn, float totalDamage, DamageInfo dinfo, DamageWorker.DamageResult result)
        {
            base.ApplySpecialEffectsToPart(pawn, totalDamage, dinfo, result);
            Pawn_SwallowWhole attacker = dinfo.Instigator as Pawn_SwallowWhole;
            if (attacker != null && attacker.Map != null && !pawn.Downed && !pawn.Dead && pawn.def.defName != "AA_PhoenixOwlcat")
            {
                CompSwallowWhole comp = attacker.TryGetComp<CompSwallowWhole>();
                if (comp!=null && attacker.innerContainer.Count < comp.Props.stomachCapacity && pawn.RaceProps.baseBodySize < comp.Props.maximumBodysize)
                {
                    attacker.needs.food.CurLevel += comp.Props.nutritionGained;
                    Patch_TakeDamage.instigatorToSet = attacker;
                    try
                    {
                        HealthUtility.DamageUntilDowned(pawn);
                    }
                    catch (Exception ex)
                    {
                        Log.Error("Failed to swallow pawn: " + ex);
                    }
                    Patch_TakeDamage.instigatorToSet = null;
                    if (comp.Props.soundPlayedWhenEating != null) {
                        SoundDef.Named(comp.Props.soundPlayedWhenEating).PlayOneShot(new TargetInfo(attacker.Position, attacker.Map, false));
                    }
                    if (comp.Props.sendLetterWhenEating&&pawn != null && pawn.Faction != null && pawn.Faction.IsPlayer)
                    {
                        Find.LetterStack.ReceiveLetter(comp.Props.letterLabel.Translate(), comp.Props.letterText.Translate(pawn), LetterDefOf.ThreatBig, attacker, null, null);
                    }
                    attacker.TryAcceptThing(pawn);
                }
            }
        }
    }
}
