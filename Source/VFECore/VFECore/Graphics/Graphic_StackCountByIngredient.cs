using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;
using UnityEngine.Diagnostics;

namespace VFECore
{
    public class Graphic_StackCountByIngredient
        : Graphic_Collection
    {

        public int cachedOffset = -1;

        public override Material MatSingle => subGraphics[subGraphics.Length - 1].MatSingle;

        public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
        {
            return GraphicDatabase.Get<Graphic_StackCount>(path, newShader, drawSize, newColor, newColorTwo, data);
        }

        public override Material MatAt(Rot4 rot, Thing thing = null)
        {
            if (thing == null)
            {
                return MatSingle;
            }
            return MatSingleFor(thing);
        }

        public override Material MatSingleFor(Thing thing)
        {
            if (thing == null)
            {
                return MatSingle;
            }
            return SubGraphicFor(thing).MatSingle;
        }

        public Graphic SubGraphicFor(Thing thing)
        {
            return SubGraphicForStackCount(thing.stackCount, thing.def, thing);
        }

        public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
        {
            Graphic graphic = (thing == null) ? subGraphics[0] : SubGraphicFor(thing);
            graphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
        }

        public Graphic SubGraphicForStackCount(int stackCount, ThingDef def, Thing thing)
        {
            int offsetToAdd = cachedOffset;
            if (offsetToAdd == -1)
            {
                offsetToAdd = GetGraphicNumberOffset(thing);
            }

            if (stackCount == 1)
            {
                return subGraphics[0 + offsetToAdd * 3];
            }
            if (stackCount == def.stackLimit)
            {
                return subGraphics[2 + offsetToAdd * 3];
            }
            return subGraphics[1 + offsetToAdd * 3];

        }

        public override string ToString()
        {
            return "StackCount(path=" + path + ", count=" + subGraphics.Length + ")";
        }

        public static int GetGraphicNumberOffset(Thing thing)
        {
            CompIngredients compIngredients = thing.TryGetComp<CompIngredients>();
            if (compIngredients != null)
            {
                foreach (ThingDef ingredient in compIngredients.ingredients)
                {
                    if (StaticCollections.graphicOffsets.ContainsKey(ingredient))
                    {
                        return StaticCollections.graphicOffsets[ingredient];
                    }
                }

            }
            return 0;
        }


    }
}


