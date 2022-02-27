using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace VFECore.Misc
{
    public class HireableFactionDef : Def
    {
        [Unsaved] private Color? cachedColor;
        public            Color  color;
        public            string commTag;

        [Unsaved] public string            editBuffer;
        public           List<PawnKindDef> pawnKinds;
        public           FactionDef        referencedFaction;
        public           string            texPath;

        [Unsaved] private Texture2D texture;

        public Color Color => referencedFaction is null
            ? color
            : (cachedColor ??
               (cachedColor = Find.World.factionManager.FirstFactionOfDef(referencedFaction).Color)).Value;

        public Texture2D Texture => texture ?? (texture = ContentFinder<Texture2D>.Get(texPath));
    }
}