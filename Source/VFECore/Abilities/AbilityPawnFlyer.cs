namespace VFECore.Abilities
{
    using System.Collections;
    using System.Collections.Generic;
    using HarmonyLib;
    using RimWorld;
    using UnityEngine;
    using Verse;

    public class AbilityPawnFlyer : PawnFlyer
    {
        public Abilities.Ability ability;

        protected Vector3 position;
        public    Vector3 target;

        public Rot4 direction;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            this.direction = this.startVec.x > this.target.ToIntVec3().x ? Rot4.West :
                             this.startVec.x < this.target.ToIntVec3().x ? Rot4.East :
                             this.startVec.y < this.target.ToIntVec3().y ? Rot4.North :
                                                                           Rot4.South;
        }

        public override void Tick()
        {
            float progress = (float) this.ticksFlying / (float) this.ticksFlightTime;
            this.position = Vector3.Lerp(this.startVec, this.target, progress) + new Vector3(0f, 0f, 2f) * GenMath.InverseParabola(progress);


            IList value = Traverse.Create(this.FlyingPawn.Drawer.renderer).Field("effecters").Field("pairs").GetValue() as IList;
            foreach (object o in value)
                Traverse.Create(o).Field("effecter").GetValue<Effecter>().EffectTick(new TargetInfo(this.position.ToIntVec3(), this.Map), TargetInfo.Invalid);

            base.Tick();
        }

        public override void DrawAt(Vector3 drawLoc, bool flip = false)
        {
            this.FlyingPawn.Drawer.renderer.RenderPawnAt(this.position, this.direction);
        }

        protected override void RespawnPawn()
        {
            this.Position = this.target.ToIntVec3();
            Pawn pawn = this.FlyingPawn;
            base.RespawnPawn();
            this.ability.ApplyHediffs(pawn);

            int? staggerTicks = this.ability.def.GetModExtension<AbilityExtension_Hediff>()?.hediff.CompProps<HediffCompProperties_Disappears>()?.disappearsAfterTicks.RandomInRange;
            if (staggerTicks.HasValue)
            {
                pawn.stances.SetStance(new Stance_Cooldown(staggerTicks.Value + 1, this.ability.CasterPawn, null));
                pawn.stances.StaggerFor(staggerTicks.Value);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref this.ability, nameof(this.ability));
            Scribe_Values.Look(ref this.direction, nameof(this.direction));
        }
    }
}