
using RimWorld;
using Verse;


namespace VEF.AnimalBehaviours
{
    public class Verb_CastAbilityCharge : Verb_CastAbility
    {

       
        protected override bool TryCastShot()
        {
            if (base.TryCastShot())
            {
                return DoCharge(CasterPawn, currentTarget, ReloadableCompSource, verbProps);
            }
            return false;

        }

        public static bool DoCharge(Pawn pawn, LocalTargetInfo currentTarget, CompApparelReloadable comp, VerbProperties verbProps, Ability triggeringAbility = null, LocalTargetInfo target = default(LocalTargetInfo), ThingDef pawnFlyerOverride = null)
        {

            IntVec3 position = pawn.Position;
            IntVec3 cell = currentTarget.Cell;
            Map map = pawn.Map;
            bool flag = Find.Selector.IsSelected(pawn);
            Pawn_Charger pawnFlyer = (Pawn_Charger)PawnFlyer.MakeFlyer(InternalDefOf.VEF_Charger, pawn, cell, verbProps.flightEffecterDef, verbProps.soundLanding, verbProps.flyWithCarriedThing, null, triggeringAbility, target);

            if (pawnFlyer != null)
            {
                pawnFlyer.damageMultiplierInCharge = verbProps.meleeDamageBaseAmount;
                FleckMaker.ThrowDustPuff(position.ToVector3Shifted() + Gen.RandomHorizontalVector(0.5f), map, 2f);
                GenSpawn.Spawn(pawnFlyer, cell, map);
                if (flag)
                {
                    Find.Selector.Select(pawn, playSound: false, forceDesignatorDeselect: false);
                }
                return true;
            }
            return false;
        }







    }
}
