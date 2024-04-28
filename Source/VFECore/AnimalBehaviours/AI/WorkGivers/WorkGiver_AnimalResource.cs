using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace AnimalBehaviours
{
    public class WorkGiver_AnimalResource : WorkGiver_GatherAnimalBodyResources
    {
        protected override JobDef JobDef
        {
            get
            {
                return InternalDefOf.VEF_AnimalResource;
            }
        }

        protected override CompHasGatherableBodyResource GetComp(Pawn animal)
        {
            return animal.TryGetComp<CompAnimalProduct>();
        }

        public override bool ShouldSkip(Pawn pawn, bool forced = false)
        {
            List<Pawn> list = pawn.Map.mapPawns.SpawnedPawnsInFaction(pawn.Faction);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].RaceProps.Animal || list[i].RaceProps.IsMechanoid)
                {
                    CompHasGatherableBodyResource comp = GetComp(list[i]);
                    if (comp != null && comp.ActiveAndFull)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
        {
            Pawn pawn2 = t as Pawn;
            if (pawn.RaceProps.IsMechanoid)
            {
                return false;
            }
            if (pawn2 == null || !(pawn2.RaceProps.Animal|| pawn2.RaceProps.IsMechanoid))
            {
                return false;
            }
            CompHasGatherableBodyResource comp = GetComp(pawn2);
            if (comp == null || !comp.ActiveAndFull || pawn2.Downed || (pawn2.roping != null && pawn2.roping.IsRopedByPawn) || !pawn2.CanCasuallyInteractNow() || !pawn.CanReserve(pawn2, 1, -1, null, forced))
            {
                return false;
            }
            return true;
        }

    }
}

