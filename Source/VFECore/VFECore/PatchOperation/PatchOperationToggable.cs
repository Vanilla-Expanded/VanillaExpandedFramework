using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Verse;

namespace VFECore
{
	public class PatchOperationToggable : PatchOperation
	{
#pragma warning disable 0649
		public bool enabled;
		public string label;
		public List<string> mods = new List<string>();
		private PatchOperation match;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			bool flag = false;
			for (int i = 0; i < this.mods.Count; i++)
			{
				if (ModLister.HasActiveModWithName(this.mods[i]))
				{
					flag = true;
				}
				else
                {
					flag = false;
					break;
                }
			}

			if (this.enabled && flag)
			{
				if (this.match != null)
				{
					return this.match.Apply(xml);
				}
			}
			return true;
		}
	}
}
