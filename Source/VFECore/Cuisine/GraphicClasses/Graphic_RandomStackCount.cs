using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;

namespace VanillaCookingExpanded
{
    public class Graphic_RandomStackCount : Graphic_Collection
    {
        // A new graphic class that allows for random graphics in a StatCount item

        // The amount of images in the folder must be a multiple of 3, or weird things will happen!

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

            int randomToAdd = thing.thingIDNumber % (subGraphics.Length / 3);

            if (stackCount == 1)
            {
                return subGraphics[0 + randomToAdd * 3];
            }
            if (stackCount == def.stackLimit)
            {
                return subGraphics[2 + randomToAdd * 3];
            }
            return subGraphics[1 + randomToAdd * 3];

        }

        public override string ToString()
        {
            return "StackCount(path=" + path + ", count=" + subGraphics.Length + ")";
        }
    }
}


