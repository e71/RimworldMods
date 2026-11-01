using HarmonyLib;
using RimWorld;
using Verse;

namespace BetterCV;

[HarmonyPatch(typeof(GenConstruct), "BlocksConstruction")]
public static class BlocksConstruction
{
	[HarmonyPostfix]
	public static void _Postfix(Thing constructible, Thing t, ref bool __result)
	{
		if (__result)
		{
			BuildableDef buildableDef = constructible.def.entityDefToBuild;
			if (buildableDef == null)
			{
				buildableDef = constructible.def;
			}
			BuildableDef buildableDef2 = t.def.entityDefToBuild;
			if (buildableDef2 == null)
			{
				buildableDef2 = t.def;
			}
			if ((BetterCVUtil.isOverWall(buildableDef) && BetterCVUtil.isWall(buildableDef2)) || (BetterCVUtil.isOverWall(buildableDef2) && BetterCVUtil.isWall(buildableDef)))
			{
				__result = false;
			}
		}
	}
}
