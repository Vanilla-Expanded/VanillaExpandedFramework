using System;
using Verse;

namespace GraphicCustomization
{
    public class TextureVariantOverride
    {
        public float chance;
        public string groupName;
        public string texName;
    }
    public class TextureVariant : IExposable, IEquatable<TextureVariant>
    {
        public string texName;
        public string texture;
        public string outline;
        public TextureVariantOverride textureVariantOverride;
        public float chanceOverride = 1f;

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
