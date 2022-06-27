using System;
using UnityEngine;
using Verse;

namespace PipeSystem
{
    public static class IconOverlay
    {
        public static void RenderPusling(Thing thing, Material mat, Vector3 drawPos, Mesh mesh)
        {
            float num = ((float)Math.Sin((Time.realtimeSinceStartup + 397f * (thing.thingIDNumber % 571)) * 4f) + 1f) * 0.5f;
            num = 0.3f + num * 0.7f;
            Material material = FadedMaterialPool.FadedVersionOf(mat, num);
            Graphics.DrawMesh(mesh, Matrix4x4.TRS(drawPos, Quaternion.identity, Vector3.one), material, 0);
        }

        public static void Render(Material mat, Vector3 drawPos, Mesh mesh)
        {
            Graphics.DrawMesh(mesh, Matrix4x4.TRS(drawPos, Quaternion.identity, Vector3.one), mat, 0);
        }
    }
}
