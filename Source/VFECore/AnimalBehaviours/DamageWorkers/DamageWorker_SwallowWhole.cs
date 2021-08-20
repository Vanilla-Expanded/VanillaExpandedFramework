using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse.Sound;
using Verse;

namespace AnimalBehaviours
{

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
                    HealthUtility.DamageUntilDowned(pawn);
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
