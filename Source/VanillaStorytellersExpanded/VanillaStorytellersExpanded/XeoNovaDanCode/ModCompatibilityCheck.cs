using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace VanillaStorytellersExpanded
{
	// Token: 0x0200001B RID: 27
	[StaticConstructorOnStartup]
	public static class ModCompatibilityCheck
	{
		// Token: 0x06000037 RID: 55 RVA: 0x00002B9C File Offset: 0x00000D9C
		static ModCompatibilityCheck()
		{
			List<ModMetaData> list = ModsConfig.ActiveModsInLoadOrder.ToList<ModMetaData>();
			for (int i = 0; i < list.Count; i++)
			{
				ModMetaData modMetaData = list[i];
				bool flag = modMetaData.Name == "Dual Wield";
				if (flag)
				{
					ModCompatibilityCheck.DualWield = true;
				}
				else
				{
					bool flag2 = modMetaData.Name == "Facial Stuff 1.1";
					if (flag2)
					{
						ModCompatibilityCheck.FacialStuff = true;
					}
					else
					{
						bool flag3 = modMetaData.Name == "Research Tree";
						if (flag3)
						{
							ModCompatibilityCheck.ResearchTree = true;
						}
						else
						{
							bool flag4 = modMetaData.Name == "ResearchPal";
							if (flag4)
							{
								ModCompatibilityCheck.ResearchPal = true;
							}
							else
							{
								bool flag5 = modMetaData.Name == "RimCities";
								if (flag5)
								{
									ModCompatibilityCheck.RimCities = true;
								}
								else
								{
									bool flag6 = modMetaData.Name == "[1.1] RPG Style Inventory";
									if (flag6)
									{
										ModCompatibilityCheck.RPGStyleInventory = true;
									}
									else
									{
										bool flag7 = modMetaData.Name == "RunAndGun";
										if (flag7)
										{
											ModCompatibilityCheck.RunAndGun = true;
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x0400002E RID: 46
		public static bool DualWield;

		// Token: 0x0400002F RID: 47
		public static bool FacialStuff;

		// Token: 0x04000030 RID: 48
		public static bool ResearchTree;

		// Token: 0x04000031 RID: 49
		public static bool ResearchPal;

		// Token: 0x04000032 RID: 50
		public static bool RimCities;

		// Token: 0x04000033 RID: 51
		public static bool RPGStyleInventory;

		// Token: 0x04000034 RID: 52
		public static bool RunAndGun;
	}
}
