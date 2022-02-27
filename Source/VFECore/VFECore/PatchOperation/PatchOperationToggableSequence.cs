using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Verse;

namespace VFECore
{
	public class PatchOperationToggableSequence : PatchOperation
	{
		public bool enabled;
		public string label;

		private readonly List<string> mods = new List<string>();
		private readonly List<PatchOperation> operations = new List<PatchOperation>();
		private PatchOperation lastFailedOperation;

		protected override bool ApplyWorker(XmlDocument xml)
		{
			if (ModsFound())
			{
				string pLabelSmall = label.Replace(" ", "");
				if (!VFEGlobal.settings.toggablePatch.NullOrEmpty() && VFEGlobal.settings.toggablePatch.ContainsKey(pLabelSmall))
				{
					VFEGlobal.settings.toggablePatch.TryGetValue(pLabelSmall, out bool v);
					if (v) return ApplyPatches(xml);
				}
				else if (this.enabled)
				{
					return ApplyPatches(xml);
				}
			}

			return true;
		}

		private bool ApplyPatches(XmlDocument xml)
        {
			foreach (PatchOperation operation in this.operations)
			{
				if (!operation.Apply(xml))
				{
					this.lastFailedOperation = operation;
					return false;
				}
			}
			return true;
		}

		public override string ToString()
		{
			int num = this.operations != null ? this.operations.Count : 0;
			string str = string.Format("{0}(count={1}", base.ToString(), num);
			if (this.lastFailedOperation != null)
				str = str + ", lastFailedOperation=" + this.lastFailedOperation;
			return str + ")";
		}

		public override void Complete(string modIdentifier)
		{
			base.Complete(modIdentifier);
			this.lastFailedOperation = null;
		}

		public bool ModsFound()
        {
			for (int index = 0; index < this.mods.Count; index++)
			{
				if (!ModLister.HasActiveModWithName(this.mods[index]))
					return false;
			}
			return true;
		}
	}
}
