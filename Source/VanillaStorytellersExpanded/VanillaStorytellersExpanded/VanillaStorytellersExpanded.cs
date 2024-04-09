using System;
using HarmonyLib;
using Verse;

namespace VanillaStorytellersExpanded
{
	// Token: 0x02000025 RID: 37
	public class VanillaStorytellersExpanded : Mod
	{
		// Token: 0x06000057 RID: 87 RVA: 0x000021F7 File Offset: 0x000003F7
		public VanillaStorytellersExpanded(ModContentPack content) : base(content)
		{
			VanillaStorytellersExpanded.harmonyInstance = new Harmony("OskarPotocki.VanillaStorytellersExpanded");
		}

		// Token: 0x04000050 RID: 80
		public static Harmony harmonyInstance;
	}
}
