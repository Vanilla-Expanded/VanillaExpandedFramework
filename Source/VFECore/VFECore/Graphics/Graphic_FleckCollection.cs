namespace VFECore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Verse;

    public abstract class Graphic_FleckCollection : Graphic_Fleck
    {
        protected Graphic_Fleck[] subGraphics;

        public override void Init(GraphicRequest req)
        {
            this.data = req.graphicData;
            if (req.path.NullOrEmpty()) throw new ArgumentNullException("folderPath");

            if (req.shader == null) throw new ArgumentNullException("shader");

            this.path     = req.path;
            this.maskPath = req.maskPath;
            this.color    = req.color;
            this.colorTwo = req.colorTwo;
            this.drawSize = req.drawSize;
            List<Texture2D> list = (from x in ContentFinder<Texture2D>.GetAllInFolder(req.path)
                                    where !x.name.EndsWith(MaskSuffix)
                                    orderby x.name
                                    select x).ToList();
            if (list.NullOrEmpty())
            {
                Log.Error("Collection cannot init: No textures found at path " + req.path);
                this.subGraphics = new Graphic_Fleck[0];
                return;
            }

            this.subGraphics = (from texture2D in list
                                select (Graphic_Fleck) GraphicDatabase.Get(typeof(Graphic_Fleck), req.path + "/" + texture2D.name, req.shader, this.drawSize,
                                                                           this.color,
                                                                           this.colorTwo, this.data, req.shaderParameters)).ToArray();
        }
    }
}