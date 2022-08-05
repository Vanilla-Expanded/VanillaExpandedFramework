namespace VFECore
{
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using UnityEngine;
    using Verse;

    public class Graphic_AnimatedMote : Graphic_Animated
    {
		public override void Init(GraphicRequest req)
		{
			data = req.graphicData;
			if (req.path.NullOrEmpty())
			{
				throw new ArgumentNullException("folderPath");
			}
			if (req.shader == null)
			{
				throw new ArgumentNullException("shader");
			}
			path = req.path;
			maskPath = req.maskPath;
			color = req.color;
			colorTwo = req.colorTwo;
			drawSize = req.drawSize;
			List<Texture2D> list = (from x in ContentFinder<Texture2D>.GetAllInFolder(req.path)
									where !x.name.EndsWith(Graphic_Single.MaskSuffix)
									orderby x.name
									select x).ToList();
			if (list.NullOrEmpty())
			{
				Log.Error("Collection cannot init: No textures found at path " + req.path);
				subGraphics = new Graphic[1] { BaseContent.BadGraphic };
				return;
			}
			List<Graphic> list2 = new List<Graphic>();
			foreach (IGrouping<string, Texture2D> item in from s in list
														  group s by s.name.Split('_')[0])
			{
				List<Texture2D> list3 = item.ToList();
				string text = req.path + "/" + item.Key;
				bool flag = false;
				for (int num = list3.Count - 1; num >= 0; num--)
				{
					if (list3[num].name.Contains("_east") || list3[num].name.Contains("_north") || list3[num].name.Contains("_west") || list3[num].name.Contains("_south"))
					{
						list3.RemoveAt(num);
						flag = true;
					}
				}
				if (list3.Count > 0)
				{
					foreach (Texture2D item2 in list3)
					{
						list2.Add(GraphicDatabase.Get(typeof(Graphic_Mote), req.path + "/" + item2.name, req.shader, drawSize, color, colorTwo, data, req.shaderParameters));
					}
				}
				if (flag)
				{
					list2.Add(GraphicDatabase.Get(typeof(Graphic_Multi), text, req.shader, drawSize, color, colorTwo, data, req.shaderParameters));
				}
			}
			subGraphics = list2.ToArray();
		}
	}
}