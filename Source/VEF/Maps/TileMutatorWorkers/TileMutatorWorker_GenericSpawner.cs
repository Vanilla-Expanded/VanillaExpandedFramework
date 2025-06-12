
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VEF.Maps
{
    public class TileMutatorWorker_GenericSpawner : TileMutatorWorker
    {




        public TileMutatorWorker_GenericSpawner(TileMutatorDef def)
            : base(def)
        {
        }

        public override void GenerateCriticalStructures(Map map)
        {
            int count;
            TileMutatorExtension extension = this.def.GetModExtension<TileMutatorExtension>();
            if (extension != null)
            {
                count = extension.thingToSpawnAmount.RandomInRange;
                int spawned = 0;
                foreach (IntVec3 cell in map.AllCells.InRandomOrder())
                {

                    Thing thing = ThingMaker.MakeThing(extension.thingToSpawn, null);
                    GenSpawn.Spawn(thing, cell, map);
                    if (++spawned >= count)
                    {
                        break;
                    }

                }
            }


        }



    }
}