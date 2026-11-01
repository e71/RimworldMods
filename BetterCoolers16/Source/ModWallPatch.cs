using System.Reflection;
using HarmonyLib;
using Verse;

namespace BetterCV;

[StaticConstructorOnStartup]
public static class ModWallPatch
{
	static ModWallPatch()
	{
		new Harmony("BetterCV").PatchAll(Assembly.GetExecutingAssembly());
	}
}
