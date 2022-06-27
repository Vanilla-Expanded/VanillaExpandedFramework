using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;

namespace VFECore.Shields
{
    public class HediffComp_DamageAura : HediffComp_Draw
    {
        private Sustainer                       sustainer;

        public  HediffCompProperties_DamageAura Props => props as HediffCompProperties_DamageAura;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            if (sustainer == null) sustainer = Props.sustainer.TrySpawnSustainer(Pawn);
            sustainer.Maintain();
            if (Pawn.IsHashIntervalTick(Props.ticksBetween))
                foreach (var thing in GenRadial.RadialDistinctThingsAround(Pawn.Position, Pawn.Map, Props.radius, true).Except(Pawn).Where(ValidateTarget))
                    thing.TakeDamage(new DamageInfo(Props.damageDef, Props.damageAmount, Props.armorPenetration, Pawn.DrawPos.AngleToFlat(thing.DrawPos),
                        Pawn));
        }

        public override void CompPostPostRemoved()
        {
            sustainer.End();
            Props.soundEnded?.PlayOneShot(Pawn);
            base.CompPostPostRemoved();
        }

        protected virtual bool ValidateTarget(Thing thing) => (!Props.hostileOnly || thing.HostileTo(Pawn)) && Props.targetingParameters.CanTarget(thing);
    }

    public class HediffCompProperties_DamageAura : HediffCompProperties_Draw
    {
        public DamageDef damageDef;
        public float     damageAmount     = -1f;
        public float     armorPenetration = -1f;
        public int       ticksBetween;
        public float     radius;
        public SoundDef  sustainer;
        public SoundDef  soundEnded;
        public bool      hostileOnly = true;


        public TargetingParameters targetingParameters = new TargetingParameters
        {
            canTargetPawns     = true,
            canTargetBuildings = true
        };
    }
}
