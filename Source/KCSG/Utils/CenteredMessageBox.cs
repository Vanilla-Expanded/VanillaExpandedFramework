using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace KCSG
{
	public class CenteredMessageBox : Window
	{
		public CenteredMessageBox(string text)
		{
			this.text = text;
			this.closeOnAccept = false;
			this.closeOnCancel = false;
		}

		public override Vector2 InitialSize
		{
			get
			{
				return new Vector2(240f, 75f);
			}
		}

		public override void DoWindowContents(Rect inRect)
		{
			TextAnchor anchor = Text.Anchor;
			Text.Anchor = TextAnchor.MiddleCenter;
			Widgets.Label(inRect, this.text);
			Text.Anchor = anchor;
		}

		public void Close()
		{
			this.Close(false);
		}

		private string text;
	}
}
