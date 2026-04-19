using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace VEF.Storyteller
{
    public class QuestPart_Site : QuestPartActivable
    {
        public MapParent mapParent;
        public Faction siteFaction;
        private int lastTileChecked = -1;
        public bool applyOnPocketMap;

        public Map Map
        {
            get
            {
                if (applyOnPocketMap) return Find.World.pocketMaps.FirstOrDefault(mp => mp.sourceMap == mapParent.Map)?.Map;
                return mapParent?.Map;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref mapParent, "mapParent");
            Scribe_References.Look(ref siteFaction, "siteFaction");
            Scribe_Values.Look(ref lastTileChecked, "lastTileChecked", -1);
            Scribe_Values.Look(ref applyOnPocketMap, "applyOnPocketMap");
            if (Scribe.mode == LoadSaveMode.PostLoadInit && mapParent != null) lastTileChecked = mapParent.Tile;
        }

        public override void QuestPartTick()
        {
            base.QuestPartTick();
            if (mapParent == null || mapParent.Destroyed)
            {
                var oldMapParent = mapParent;
                if (lastTileChecked != -1)
                {
                    var newMapParent = Find.WorldObjects.MapParentAt(lastTileChecked);
                    mapParent = newMapParent != null && newMapParent != mapParent ? newMapParent : null;
                }
                else mapParent = null;
                if (oldMapParent != null && oldMapParent != mapParent && oldMapParent.Destroyed)
                {
                    mapParent.questTags ??= new List<string>();
                    mapParent.questTags.AddRange(oldMapParent.questTags);
                }
            }
            else if (lastTileChecked == -1) lastTileChecked = mapParent.Tile;
        }
    }
}