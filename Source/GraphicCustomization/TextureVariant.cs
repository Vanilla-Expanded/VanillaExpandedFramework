using System;
using Verse;

namespace GraphicCustomization
{
    public class TextureVariant : IExposable, IEquatable<TextureVariant>
    {
        public string texName;
        public string texture;
        public string outline;
        public bool Equals(TextureVariant other)
        {
            return texName == other.texName && texture == other.texture && outline == other.outline;
        }
        public void ExposeData()
        {
            Scribe_Values.Look(ref texName, "texName");
            Scribe_Values.Look(ref texture, "texture");
            Scribe_Values.Look(ref outline, "outline");
        }
    }
}
