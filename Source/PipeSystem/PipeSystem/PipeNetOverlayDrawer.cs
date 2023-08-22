using System;
using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    public class PipeNetOverlayDrawer : MapComponent
    {
        private readonly Dictionary<Thing, Material> pulsingRender = new Dictionary<Thing, Material>();
        private readonly Dictionary<Vector3, Material> staticRender = new Dictionary<Vector3, Material>();

        public PipeNetOverlayDrawer(Map map) : base(map)
        {
        }

        public void TogglePulsing(Thing thing, Material mat, bool val)
        {
            if (mat == null)
                return;

            var isInDic = pulsingRender.ContainsKey(thing);
            if (val && !isInDic)
            {
                pulsingRender.Add(thing, mat);
            }
            else if (!val && isInDic)
            {
                pulsingRender.Remove(thing);
            }
        }

        public void ToggleStatic(Thing thing, Material mat, bool val)
        {
            if (mat == null)
                return;

            var vec = thing.TrueCenter();
            var isInDic = staticRender.ContainsKey(vec);
            if (val && !isInDic)
            {
                staticRender.Add(vec, mat);
            }
            else if (!val && isInDic)
            {
                staticRender.Remove(vec);
            }
        }

        public override void MapComponentUpdate()
        {
            if (WorldRendererUtility.WorldRenderedNow || Find.CurrentMap != map)
                return;

            foreach (var pulsing in pulsingRender)
            {
                var thing = pulsing.Key;
                RenderPusling(thing, pulsing.Value, thing.TrueCenter(), MeshPool.plane08);
            }
            foreach (var stat in staticRender)
            {
                RenderStatic(stat.Value, stat.Key, MeshPool.plane08);
            }
        }

        private void RenderPusling(Thing thing, Material mat, Vector3 drawPos, Mesh mesh)
        {
            float num = ((float)Math.Sin((Time.realtimeSinceStartup + 397f * (thing.thingIDNumber % 571)) * 4f) + 1f) * 0.5f;
            num = 0.3f + num * 0.7f;
            Material material = FadedMaterialPool.FadedVersionOf(mat, num);
            Graphics.DrawMesh(mesh, Matrix4x4.TRS(drawPos, Quaternion.identity, Vector3.one), material, 0);
        }

        private void RenderStatic(Material mat, Vector3 drawPos, Mesh mesh)
        {
            Graphics.DrawMesh(mesh, Matrix4x4.TRS(drawPos, Quaternion.identity, Vector3.one), mat, 0);
        }
    }
}