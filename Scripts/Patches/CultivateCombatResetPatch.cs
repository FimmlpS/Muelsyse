using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Muelsyse.Scripts.UI;

namespace Muelsyse.Scripts.Patches;

/// <summary>
/// A save &amp; load throws the old combat away and builds a brand-new room, but everything the
/// cultivation keeps in static fields — the session, the overlay layer, and the task awaiting the
/// material selection — still refers to the fight that no longer exists. That task may never resume
/// (the choice it was waiting on dies with the combat), so nothing would ever dismiss the overlay it
/// left on the screen, and the freed node it still points at would then break the next cultivation.
/// A room being readied always means no cultivation is in flight, so drop all of it.
/// </summary>
[HarmonyPatch(typeof(NCombatRoom), nameof(NCombatRoom._Ready))]
public static class CultivateCombatResetPatch
{
    [HarmonyPostfix]
    public static void Postfix()
    {
        CultivateOverlay.Reset();
    }
}
