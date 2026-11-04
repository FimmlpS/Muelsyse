using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace Muelsyse.Scripts.UI;

[HarmonyPatch(typeof(NCard), nameof(NCard.UpdateVisuals))]
public static class CultivateCardPatch
{
    [HarmonyPostfix]
    public static void Postfix(NCard __instance, PileType pileType, CardPreviewMode previewMode)
    {
        CultivateCardDisplay.Refresh(__instance);
    }
}

/// <summary>
/// Card nodes are pooled: the same <see cref="NCard"/> is bound to a different model over and over.
/// Assigning <see cref="NCard.Model"/> runs Reload → ReloadOverlay, which rebuilds the vanilla
/// overlay but knows nothing about the children we add. Clear them there so a dried/cultivated
/// icon can never leak onto an unrelated card.
/// </summary>
[HarmonyPatch(typeof(NCard), "ReloadOverlay")]
public static class CultivateOverlayResetPatch
{
    [HarmonyPostfix]
    public static void Postfix(NCard __instance)
    {
        CultivateCardDisplay.Clear(__instance);
    }
}
