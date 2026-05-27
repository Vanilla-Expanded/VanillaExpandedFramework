using RimWorld;
using System;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace VEF.AnimalBehaviours
{
    public class DeathActionWorker_SpawnItemsAndFilth : DeathActionWorker
    {

        public DeathActionProperties_SpawnItemsAndFilth Props => (DeathActionProperties_SpawnItemsAndFilth)props;


        public override void PawnDied(Corpse corpse, Lord prevLord)
        {
            if (corpse.Map != null)
            {
                if (Rand.Chance(Props.dropChance))
                {
                    ThingDefCount thingDefCount;
                    if (Props.isRandom)
                    {
                        thingDefCount = Props.items.RandomElement();
                        if (thingDefCount != null)
                        {
                            Thing thing = ThingMaker.MakeThing(thingDefCount.ThingDef);
                            thing.stackCount = thingDefCount.Count;
                            GenPlace.TryPlaceThing(thing, corpse.Position, corpse.Map, ThingPlaceMode.Near, null, null, default(Rot4));
                        }
                    }
                    else
                    {
                        foreach (ThingDefCount thingDefCount2 in Props.items)
                        {
                            Thing thing = ThingMaker.MakeThing(thingDefCount2.ThingDef);
                            thing.stackCount = thingDefCount2.Count;
                            GenPlace.TryPlaceThing(thing, corpse.Position, corpse.Map, ThingPlaceMode.Near, null, null, default(Rot4));
                        }
                    }
                    for (int i = 0; i < Props.filthCountRange.RandomInRange; i++)
                    {
                        IntVec3 c;
                        CellFinder.TryFindRandomReachableNearbyCell(corpse.PositionHeld, corpse.MapHeld, 2, TraverseParms.For(TraverseMode.NoPassClosedDoors, Danger.Deadly, false), null, null, out c);
                        FilthMaker.TryMakeFilth(c, corpse.MapHeld, Props.filthCreated);
                    }
                    if (Props.sound != null)
                    {
                        Props.sound.PlayOneShot(new TargetInfo(corpse.PositionHeld, corpse.MapHeld, false));
                    }
                }
            }
        }
    }
}
