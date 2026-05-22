
using RimWorld;
using VEF.AnimalBehaviours;
using Verse;
using Verse.AI.Group;

namespace VEF.AnimalBehaviours
{
    public class DeathActionWorker_VanishInsect : DeathActionWorker
    {
        public DeathActionProperties_VanishInsect Props => (DeathActionProperties_VanishInsect)props;

        public override void PawnDied(Corpse corpse, Lord prevLord)
        {
            if (Props.fleck != null)
            {
                FleckMaker.Static(corpse.PositionHeld, corpse.MapHeld, Props.fleck);
            }
            if (Props.filth != null)
            {
                int randomInRange = Props.filthCountRange.RandomInRange;
                for (int i = 0; i < randomInRange; i++)
                {
                    FilthMaker.TryMakeFilth(corpse.PositionHeld, corpse.MapHeld, Props.filth);
                }
            }

            CellRect cellRect = new CellRect(corpse.PositionHeld.x, corpse.PositionHeld.z, 3, 3).ClipInsideMap(corpse.MapHeld);

            IntVec3 randomCell = cellRect.RandomCell;
            ThingDef filthDef = ThingDefOf.Filth_BloodInsect;
            if (randomCell.InBounds(corpse.MapHeld) && GenSight.LineOfSight(randomCell, corpse.PositionHeld, corpse.MapHeld))
            {
                FilthMaker.TryMakeFilth(randomCell, corpse.MapHeld, filthDef);
            }



            corpse.Destroy();
        }
    }
}