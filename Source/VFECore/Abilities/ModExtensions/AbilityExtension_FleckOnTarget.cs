using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace VFECore.Abilities
{
    public class AbilityExtension_FleckOnTarget : AbilityExtension_AbilityMod
    {
        public FleckDef       fleckDef;
        public List<FleckDef> fleckDefs;
        public SoundDef       sound;
        public float          scale        = 1f;
        public int            preCastTicks = -1;

        public override void Cast(LocalTargetInfo target, Ability ability)
        {
            base.Cast(target, ability);
            if (preCastTicks <= 0) SpawnAll(target.ToTargetInfo(ability.pawn.Map));
        }

        public override void WarmupToil(Toil toil)
        {
            base.WarmupToil(toil);
            if (preCastTicks > 0)
                toil.AddPreTickAction(delegate
                {
                    if (toil.actor.jobs.curDriver.ticksLeftThisToil == preCastTicks)
                        SpawnAll(toil.actor.jobs.curJob.GetTarget(TargetIndex.A).ToTargetInfo(toil.actor.Map));
                });
        }

        private void SpawnAll(TargetInfo target)
        {
            if (!fleckDefs.NullOrEmpty())
                for (var i = 0; i < fleckDefs.Count; i++)
                    SpawnFleck(target, fleckDefs[i]);

            if (fleckDef != null) SpawnFleck(target, fleckDef);

            sound?.PlayOneShot(target);
        }

        private void SpawnFleck(TargetInfo target, FleckDef def)
        {
            if (target.HasThing)
                FleckMaker.AttachedOverlay(target.Thing, def, Vector3.zero, scale);
            else
                FleckMaker.Static(target.Cell, target.Map, def, scale);
        }
    }
}