using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;
using Verse;
using System.Linq;



namespace VanillaFurnitureExpanded
{
    [StaticConstructorOnStartup]
    public class Command_SetStoneType : Command
    {


        public CompRockSpawner building;



        public Command_SetStoneType()
        {

            defaultDesc = "VFE_ChooseMineDesc".Translate();
            defaultLabel = "VFE_ChooseMine".Translate();
            this.icon = ContentFinder<Texture2D>.Get("UI/Commands/VFE_RandomChunks", true);




        }



        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            IEnumerable<ThingDef> rocksInThisBiome = Find.World.NaturalRockTypesIn(building.parent.Map.Tile);
            List<ThingDef> chunksInThisBiome = new List<ThingDef>();
            foreach (ThingDef rock in rocksInThisBiome)
            {
                chunksInThisBiome.Add(rock.building.mineableThing);
            }

            list.Add(new FloatMenuOption("VFE_ChunkRandomMine".Translate(), delegate
            {
                building.RockTypeToMine = null;

            }, MenuOptionPriority.Default, null, null, 29f, null, null));

            foreach(ThingDef chunk in chunksInThisBiome)
            {
                list.Add(new FloatMenuOption("VFE_ChunkToMine".Translate(chunk.LabelCap), delegate
                {
                    building.RockTypeToMine = chunk;

                }, MenuOptionPriority.Default, null, null, 29f, null, null));
            }          

            Find.WindowStack.Add(new FloatMenu(list));
        }






    }


}


