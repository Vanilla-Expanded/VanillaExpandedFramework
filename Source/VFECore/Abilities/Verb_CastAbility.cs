using RimWorld.Planet;

namespace VFECore.Abilities
{
    using Verse;
    using Verse.AI;

    public class Verb_CastAbility : Verb
    {
        public Ability ability;

        protected override bool TryCastShot()
        {
            if (this.ability.IsEnabledForPawn(out _))
            {
                this.ability.Cast(this.currentTarget.IsValid ? this.currentTarget : this.CurrentDestination);
                return true;
            }

            return false;
        }

        private void StartAbilityJob(LocalTargetInfo castTarg, LocalTargetInfo destTarg)
        {
            if (castTarg.IsValid && !destTarg.IsValid) this.ability.CreateCastJob(castTarg);
            if (!castTarg.IsValid && destTarg.IsValid) this.ability.CreateCastJob(destTarg);
            if (castTarg.IsValid && destTarg.IsValid)
            {
                this.ability.CreateCastJob(castTarg.ToGlobalTargetInfo(this.ability.pawn.Map),
                destTarg.ToGlobalTargetInfo(this.ability.pawn.Map));
            }
        }

        public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false, bool nonInterruptingSelfCast = false)
        {
            if (base.TryStartCastOn(castTarg, destTarg, surpriseAttack, canHitNonTargetPawns, preventFriendlyFire))
            {
                StartAbilityJob(castTarg, destTarg);
                return true;
            }
            return false;
        }

        public override void OrderForceTarget(LocalTargetInfo target)
        {
            base.OrderForceTarget(target);
            this.ability.currentTargetingIndex = -1;
            this.ability.currentTargets = new GlobalTargetInfo[this.ability.def.targetCount];
            this.ability.OrderForceTarget(target);
        }
        public override void OnGUI(LocalTargetInfo target)
        {
            DrawAttachmentExtraLabel(target);
        }
        protected void DrawAttachmentExtraLabel(LocalTargetInfo target)
        {
            foreach (var modExtension in ability.AbilityModExtensions)
            {
                string text = modExtension.ExtraLabelMouseAttachment(target, ability);
                if (!text.NullOrEmpty())
                {
                    Widgets.MouseAttachedLabel(text);
                    break;
                }
            }
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref this.ability, nameof(this.ability));
        }
    }
}
