using RimWorld;
using Verse;
using Verse.AI.Group;

namespace AnimalBehaviours
{
    public class DeathActionWorker_ConfigurableExplosion : DeathActionWorker
    {

        public DeathActionProperties_ConfigurableExplosion Props => (DeathActionProperties_ConfigurableExplosion)props;

        public override RulePackDef DeathRules => RulePackDefOf.Transition_DiedExplosive;

        public override bool DangerousInMelee => true;

        public override void PawnDied(Corpse corpse, Lord prevLord)
        {
            GenExplosion.DoExplosion(radius: (corpse.InnerPawn.ageTracker.CurLifeStageIndex == 0) ? Props.babyExplosionRadius : ((corpse.InnerPawn.ageTracker.CurLifeStageIndex != 1) 
                ? Props.adultExplosionRadius : Props.juvenileExplosionRadius), center: corpse.Position, map: corpse.Map, damType: Props.damageDef, instigator: corpse.InnerPawn, damAmount: Props.damAmount,
                armorPenetration: Props.armorPenetration,explosionSound: Props.explosionSound);
        }
    }
}
