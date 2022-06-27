using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace VFECore.Abilities
{
    public class AbilityExtension_FleckOnTarget : AbilityExtension_AbilityMod
    {
        public bool           allTargets;
        public FleckDef       fleckDef;
        public List<FleckDef> fleckDefs;
        public int            preCastTicks = -1;
        public float          scale        = 1f;
        public SoundDef       sound;
        public bool           tryCenter;

        public override IEnumerable<string> ConfigErrors()
        {
            if (allTargets && tryCenter) yield return $"{nameof(AbilityExtension_FleckOnTarget)}: cannot set both allTargets and tryCenter";
        }

        public override void Cast(GlobalTargetInfo[] targets, Ability ability)
        {
            base.Cast(targets, ability);
            if (preCastTicks <= 0) SpawnAll(targets, ability);
        }

        public override void WarmupToil(Toil toil)
        {
            base.WarmupToil(toil);
            if (preCastTicks > 0)
                toil.AddPreTickAction(delegate
                {
                    if (toil.actor.jobs.curDriver.ticksLeftThisToil == preCastTicks)
                    {
                        var compAbilities = toil.actor.GetComp<CompAbilities>();
                        SpawnAll(compAbilities.currentlyCastingTargets, compAbilities.currentlyCasting);
                    }
                });
        }


        private void SpawnAll(GlobalTargetInfo[] targets, Ability ability)
        {
            if (allTargets)
                for (var i = 0; i < targets.Length; i++)
                    SpawnOn(targets[i]);
            else if (tryCenter)
                SpawnOn(ability.firstTarget.ToGlobalTargetInfo(targets[0].Map));
            else
                SpawnOn(targets[0]);
        }

        private void SpawnOn(GlobalTargetInfo target)
        {
            if (!fleckDefs.NullOrEmpty())
                for (var i = 0; i < fleckDefs.Count; i++)
                    SpawnFleck(target, fleckDefs[i]);

            if (fleckDef != null) SpawnFleck(target, fleckDef);

            sound?.PlayOneShot(target.HasThing ? target.Thing : new TargetInfo(target.Cell, target.Map));
        }

        private void SpawnFleck(GlobalTargetInfo target, FleckDef def)
        {
            if (target.HasThing)
                FleckMaker.AttachedOverlay(target.Thing, def, Vector3.zero, scale);
            else
                FleckMaker.Static(target.Cell, target.Map, def, scale);
        }
    }
}