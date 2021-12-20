using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace VFECore.Misc
{
    public class HireableFactionDef : Def
    {
        public Color color;
        public string commTag;
        [Unsaved()]
        private Material material;
        public List<PawnKindDef> pawnKinds;
        public string texPath;
        [Unsaved()]
        private Texture2D texture;
        [Unsaved()]
        public string EditBuffer;
        public Texture2D Texture => texture ?? (texture = ContentFinder<Texture2D>.Get(texPath));
        public Material Material => material ?? (material = MaterialPool.MatFrom(Texture, ShaderDatabase.DefaultShader, color));
    }
}