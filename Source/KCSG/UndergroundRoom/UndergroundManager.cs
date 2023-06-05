using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace KCSG.UndergroundRoom
{
    public class UndergroundManager : WorldComponent
    {
        private Dictionary<CompUndergroundPassage, MapParent> mapsParent = new Dictionary<CompUndergroundPassage, MapParent>();

        private List<CompUndergroundPassage> workingl1 = new List<CompUndergroundPassage>();
        private List<MapParent> workingl2 = new List<MapParent>();

        public UndergroundManager(World world) : base(world) { }

        public override void ExposeData()
        {
            Scribe_Collections.Look(ref mapsParent, "mapsParent", LookMode.Reference, LookMode.Reference, ref workingl1, ref workingl2);
        }

        public void Enter(CompUndergroundPassage passage, Pawn pawn)
        {
            if (passage.otherSide == null)
            {
                LongEventHandler.QueueLongEvent(delegate
                {
                    var mapParent = (MapParent)WorldObjectMaker.MakeWorldObject(AllDefOf.KCSG_UndergroundRoom);
                    mapParent.Tile = passage.parent.Map.Tile;
                    mapParent.SetFaction(passage.parent.Map.ParentFaction);
                    Find.WorldObjects.Add(mapParent);

                    mapsParent.Add(passage, mapParent);

                    var layout = passage.Props.mapLayouts.RandomElement();
                    var mapSize = new IntVec3(layout.sizes.x, 1, layout.sizes.z);

                    var map = MapGenerator.GenerateMap(mapSize, mapParent, mapParent.MapGeneratorDef);
                    LongEventHandler.ExecuteWhenFinished(() =>
                    {
                        var cellRect = CellRect.CenteredOn(map.Center, layout.sizes.x, layout.sizes.z);

                        GenOption.GetAllMineableIn(cellRect, map);
                        layout.Generate(cellRect, map);

                        foreach (var cell in cellRect)
                        {
                            var list = cell.GetThingList(map);
                            for (int i = 0; i < list.Count; i++)
                            {
                                var thing = list[i];
                                if (thing.TryGetComp<CompUndergroundPassage>() is CompUndergroundPassage oPassage)
                                {
                                    oPassage.otherSide = passage;
                                    passage.otherSide = oPassage;
                                }
                            }
                        }

                        if (passage.otherSide == null)
                            Log.Warning("KCSG - Layout doesn't contains any way out");

                        PassTo(pawn, passage.otherSide.Map, passage.otherSide.parent.Position);
                    });
                }, "GeneratingMapForNewEncounter", doAsynchronously: false, null);
            }
            else
            {
                LongEventHandler.QueueLongEvent(() =>
                {
                    PassTo(pawn, passage.otherSide.Map, passage.otherSide.parent.Position);
                }, "GeneratingMapForNewEncounter", doAsynchronously: false, null);
            }
        }

        private void PassTo(Pawn pawn, Map map, IntVec3 pos)
        {
            pawn.DeSpawn();
            if (!RCellFinder.TryFindRandomCellNearWith(pos, c => c.Standable(map), map, out IntVec3 cell))
            {
                Log.Error("KCSG - PassPassage cell finder error");
                return;
            }

            GenSpawn.Spawn(pawn, cell, map);
        }

        public void Despawn(CompUndergroundPassage passage)
        {
            mapsParent[passage].Abandon();
            mapsParent.Remove(passage);
        }
    }
}
