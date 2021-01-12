using RimWorld;
using Verse;

namespace MVCF.Verbs
{
    public class Verb_Jump : RimWorld.Verb_Jump
    {
        protected override bool TryCastShot()
        {
            if (!ModLister.RoyaltyInstalled)
            {
                Log.ErrorOnce(
                    "Items with jump capability are a Royalty-specific game system. If you want to use this code please check ModLister.RoyaltyInstalled before calling it. See rules on the Ludeon forum for more info.",
                    550187797);
                return false;
            }

            var casterPawn = CasterPawn;

            var cell = currentTarget.Cell;
            var map = casterPawn.Map;
            var pawnFlyer = PawnFlyer.MakeFlyer(ThingDefOf.PawnJumper, casterPawn, cell);
            if (pawnFlyer != null)
            {
                GenSpawn.Spawn(pawnFlyer, cell, map);
                return true;
            }

            return false;
        }
    }
}