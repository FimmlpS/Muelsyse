using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;

namespace Muelsyse.Scripts.Patches;

/// <summary>
/// 蜂群术士's <see cref="PersonalHivePower"/> gives the attacker a Dazed whenever it is hit, by
/// creating the card for <c>dealer.Player</c>. Pets have no <c>Player</c> of their own (they only
/// carry a <c>PetOwner</c>), so a manifold repeating a 协同 attack hands the hive a null owner:
/// <c>CreateCard&lt;Dazed&gt;(null)</c> produces an ownerless card and
/// <c>CardPileCmd.AddGeneratedCardsToCombat</c> then throws a NullReferenceException reading
/// <c>card.Owner.Creature.CombatState</c>, aborting the whole card play.
///
/// The base game already special-cases 奥斯缇 (Osty) in this exact method by resolving the pet to
/// its summoner; do the same for every pet, which also covers the mod's manifolds.
/// </summary>
[HarmonyPatch(typeof(PersonalHivePower), nameof(PersonalHivePower.AfterDamageReceived))]
public static class PersonalHivePetDealerPatch
{
    [HarmonyPrefix]
    public static void Prefix(ref Creature? dealer)
    {
        if (dealer is { Player: null, PetOwner: { } owner })
        {
            dealer = owner.Creature;
        }
    }
}
