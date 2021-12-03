using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace VFECore.SoundTest
{
    [StaticConstructorOnStartup]
    public class TextureButton
    {
        public static readonly Texture2D SoundNote = ContentFinder<Texture2D>.Get("UI/Widgets/Note");
    }
}
