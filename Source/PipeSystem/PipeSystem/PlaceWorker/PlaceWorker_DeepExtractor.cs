using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    public class PlaceWorker_DeepExtractor : PlaceWorker_ShowDeepResources
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            var def = checkingDef as ThingDef;
            var comp = def?.GetCompProperties<CompProperties_DeepExtractor>();

            if (comp == null || def == null)
                return false;

            IntVec3 cell;
            // Check if any cell is on top of deepchem
            var res = map.deepResourceGrid.ThingDefAt(loc);
            if (res == null || res.defName != comp.deepThing.defName)
                return (AcceptanceReport)"PipeSystem_CantPlaceHere".Translate(def.label, comp.deepThing.label);

            // Draw
            cell = loc;
            if (cell != IntVec3.Invalid)
            {
                var good = new List<IntVec3>();
                var treated = new HashSet<IntVec3>();
                var toCheck = new Queue<IntVec3>();

                toCheck.Enqueue(cell);
                treated.Add(cell);

                while (toCheck.Count > 0)
                {
                    var temp = toCheck.Dequeue();
                    good.Add(temp);

                    var neighbours = GenAdjFast.AdjacentCellsCardinal(temp);
                    for (int i = 0; i < neighbours.Count; i++)
                    {
                        var n = neighbours[i];
                        if (!treated.Contains(n) && map.deepResourceGrid.ThingDefAt(n) is ThingDef r && r.defName == comp.deepThing.defName)
                        {
                            treated.Add(n);
                            toCheck.Enqueue(n);
                        }
                    }
                }
                GenDraw.DrawFieldEdges(good, Color.white);
            }

            return true;
        }

        public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
        {
            base.DrawGhost(def, center, rot, ghostCol, thing);
            if (thing != null && thing.TryGetComp<CompDeepExtractor>() is CompDeepExtractor e)
            {
                GenDraw.DrawFieldEdges(e.lumpCells, Color.white);
            }
        }
    }
}
