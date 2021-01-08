using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace KCSG
{
    [StaticConstructorOnStartup]
    internal static class TextureLoader
    {
        public static readonly Texture2D helpIcon = ContentFinder<Texture2D>.Get("UI/CSG/help");
    }
}
