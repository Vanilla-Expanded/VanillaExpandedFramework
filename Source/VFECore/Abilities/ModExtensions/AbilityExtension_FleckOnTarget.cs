namespace VFECore.Abilities
{
    using RimWorld;
    using UnityEngine;
    using Verse;
    using Verse.Sound;

    public class AbilityExtension_FleckOnTarget : AbilityExtension_AbilityMod
    {
        public FleckDef fleckDef;
        public SoundDef sound;
        public float scale = 1f;
        public override void Cast(LocalTargetInfo target, Ability ability)
        {
            base.Cast(target, ability);
            if (target.HasThing)
            {
                FleckMaker.AttachedOverlay(target.Thing, fleckDef, Vector3.zero, scale);
            }
            else
            {
                FleckMaker.Static(target.Cell, ability.pawn.Map, fleckDef, scale);
            }
            sound?.PlayOneShot(new TargetInfo(target.Cell, ability.pawn.Map));
        }
    }
}