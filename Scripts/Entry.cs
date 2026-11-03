using Godot.Bridge;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace Muelsyse.Scripts;

[ModInitializer("Init")]
public class Entry
{
	private static Harmony? _harmony;

	public static void Init()
	{
		//ModConfigRegistry.Register("Muelsyse", new MuelsyseModConfig());

		_harmony = new Harmony("sts2.fimmlps.muelsyse");

		_harmony.PatchAll();
		ScriptManagerBridge.LookupScriptsInAssembly(typeof(Entry).Assembly);
		Log.Debug("Muelsyse Mod initialized!");
	}
}
