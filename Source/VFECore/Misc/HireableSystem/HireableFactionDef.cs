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

        public Color Color =>
            this.referencedFaction is null ? 
                this.color : 
                this.cachedColor ??= Find.World.factionManager.FirstFactionOfDef(this.referencedFaction)?.Color ?? this.color;

        public Texture2D Texture => this.texture ??= ContentFinder<Texture2D>.Get(this.texPath);
    }
}