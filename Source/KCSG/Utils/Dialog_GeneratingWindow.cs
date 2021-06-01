using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace KCSG
{
    public sealed class Dialog_GeneratingWindow : Window
    {
        private Color boxColor = new Color(0.13f, 0.14f, 0.16f);

        public DateTime startTime;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(100f, 110f);
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;

            Widgets.DrawBoxSolid(new Rect(0, 0, 700, 50), boxColor);
            Widgets.Label(new Rect(0, 0, InitialSize.x, InitialSize.y / 2), "Generating structures");

            Widgets.DrawBoxSolid(new Rect(710, 0, 50, 50), boxColor);
            Widgets.Label(new Rect(0, 10f + (InitialSize.y / 2), InitialSize.x, InitialSize.y / 2), (DateTime.Now - startTime).ToString());

            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
