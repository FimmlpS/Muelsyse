using BaseLib.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using Muelsyse.Scripts.Powers;
using Muelsyse.Scripts.Relics;
using Muelsyse.Scripts.UI;

namespace Muelsyse.Scripts.Mechanics.Cultivate;

/// <summary>
/// The cultivation engine. One call = pick one main material from hand, auto-fill
/// the remaining materials, apply their calculators to a target card, then consume them.
/// </summary>
public static class CultivateCmd
{
    public static readonly LocString TargetPrompt =
        new("card_selection", "CULTIVATE_TARGET");

    public static readonly LocString MainPrompt =
        new("card_selection", "CULTIVATE_MAIN");

    public static bool CanBeMaterial(CardModel card, CardModel? target)
    {
        return card != target
               && card.Pile?.Type == PileType.Hand
               && !CultivateDryModifier.IsDry(card);
    }

    public static List<CardModel> HandMaterials(Player player, CardModel? target)
    {
        return player.PlayerCombatState?.Hand.Cards
            .Where(card => CanBeMaterial(card, target))
            .ToList() ?? [];
    }

    private static bool IsUnlimited(Player player, CardModel card)
    {
        return card is ICultivateUnlimited || (IsSoilExhaust(player) && IsStatusOrCurse(card));
    }

    private static bool IsExhaustMaterial(Player player, CardModel card)
    {
        return card is ICultivateExhaust || (IsSoilExhaust(player) && IsStatusOrCurse(card));
    }

    private static bool IsSoilExhaust(Player player) =>
        player.Creature.GetPower<SoilExhaustPower>() != null;

    private static bool IsStatusOrCurse(CardModel card) =>
        card.Type is CardType.Status or CardType.Curse;

    /// <summary>
    /// StS1 auto-fill: 养料 and 枯竭 cards are taken first; every 枯竭 card raises the cap.
    /// Remaining slots are filled with the rest of the hand in hand order.
    /// </summary>
    public static List<CardModel> AutoFill(Player player, CardModel target, CardModel main, int amount)
    {
        List<CardModel> pool = HandMaterials(player, target)
            .Where(card => card != main)
            .ToList();

        int cap = amount + pool.Count(card => IsUnlimited(player, card));
        List<CardModel> subs = [];

        foreach (CardModel card in pool)
        {
            if (subs.Count >= cap)
            {
                break;
            }

            if (card is ICultivateNutrient || IsUnlimited(player, card))
            {
                subs.Add(card);
            }
        }

        foreach (CardModel card in pool)
        {
            if (subs.Count >= cap)
            {
                break;
            }

            if (!subs.Contains(card))
            {
                subs.Add(card);
            }
        }

        return subs;
    }

    public static CultivateBonus BuildBonus(CardModel target, CardModel main, IReadOnlyList<CardModel> subs,
        bool doubleMain)
    {
        CultivateBonus bonus = new(target);
        AddMaterial(bonus, main, doubleMain ? 2 : 1);
        foreach (CardModel card in subs)
        {
            AddMaterial(bonus, card, 1);
        }

        // 土壤改造：散孢 plants that many random pollen on whatever is cultivated.
        Player? player = target.Owner;
        if (player?.Creature.GetPower<SoilSpreadPower>() is { Amount: > 0 } spread)
        {
            for (int i = 0; i < spread.Amount; i++)
            {
                bonus.Apply(CultivateCalculator.Add(PollenCmd.RandomFlower(player), 1));
            }
        }

        return bonus;
    }

    private static void AddMaterial(CultivateBonus bonus, CardModel material, int repetitions)
    {
        for (int i = 0; i < repetitions; i++)
        {
            bonus.AddMaterialBase();

            if (material is ICultivateNutrient nutrient)
            {
                foreach (CultivateCalculator calculator in nutrient.NutrientCalculators)
                {
                    bonus.Apply(calculator);
                }
            }

            // 枯竭 cards pass on the cultivation effects already printed on them.
            if (material is ICultivateExhaust
                && material.GetModifier<CultivateModifier>() is { } carried)
            {
                bonus.Carry(carried);
            }
        }
    }

    /// <summary>
    /// Writes a computed bonus onto the target card for the rest of the combat. Cultivation never
    /// touches the card's deck copy: there is no out-of-combat growth, so a cultivated card returns
    /// to its printed values once the fight ends.
    /// </summary>
    public static void Apply(CardModel target, CultivateBonus bonus)
    {
        if (bonus.IsEmpty)
        {
            return;
        }

        CultivateModifier? modifier = EnsureModifier(target);
        if (modifier != null)
        {
            modifier.Add(bonus);
            CultivateModifier.BakeStats(target, bonus.BlockDelta, bonus.CostDelta);
        }

        RefreshVisuals(target);
    }

    /// <summary>Adds a single cultivated value to a card outside of the usual cultivation flow.</summary>
    public static void AddBonus(CardModel target, CultivateAim aim, int amount)
    {
        if (amount == 0)
        {
            return;
        }

        EnsureModifier(target)?.Add(aim, amount);
        RefreshVisuals(target);
    }

    /// <summary>Removes 失肥 from a card and immediately refreshes its node.</summary>
    public static void RemoveDry(CardModel card)
    {
        if (CultivateDryModifier.Remove(card))
        {
            RefreshVisuals(card);
        }
    }

    public static void RefreshVisuals(CardModel card)
    {
        NCard? node = NCard.FindOnTable(card);
        node?.UpdateVisuals(card.Pile?.Type ?? PileType.Hand, CardPreviewMode.Normal);
    }

    private static CultivateModifier? EnsureModifier(CardModel card)
    {
        CultivateModifier? modifier = card.GetModifier<CultivateModifier>();
        if (modifier != null)
        {
            return modifier;
        }

        card.AddModifier<CultivateModifier>();
        return card.GetModifier<CultivateModifier>();
    }

    /// <summary>Picks the card to cultivate.</summary>
    /// <remarks>
    /// Neither cultivation selection hands the game a <c>source</c> model, so the picked card goes
    /// back into the hand as soon as the selection closes. With a source, the game keeps the card
    /// out of its hand slot until that source reports <c>ExecutionFinished</c>, and registers one
    /// restore per selection; cultivation runs two hand selections from the same card in a row, so
    /// both restores fire together and add the same card node twice, leaving an empty holder (a
    /// blank card in hand) behind. The restore is also too late for the material that the
    /// cultivation consumes right after the selection — and for a card that ends the turn, it can
    /// race with the end-of-turn discard.
    /// </remarks>
    public static async Task<CardModel?> SelectTarget(PlayerChoiceContext choiceContext, Player player)
    {
        IEnumerable<CardModel> picked = await CardSelectCmd.FromHand(
            choiceContext, player, new CardSelectorPrefs(TargetPrompt, 1),
            card => card.Pile?.Type == PileType.Hand, null!);
        return picked.FirstOrDefault();
    }

    /// <summary>Runs a full cultivation of <paramref name="target"/>.</summary>
    public static async Task<bool> Cultivate(PlayerChoiceContext choiceContext, Player player, CardModel target,
        int amount, AbstractModel source, bool retain = false, bool doubleMain = false,
        PileType destination = PileType.Hand)
    {
        // A brand-new card only joins the hand when the hand is where it is going to stay. Cards
        // that belong in another pile (举星 is planted straight into the draw pile) stay outside the
        // piles during the selection — the overlay previews work off the model, not a hand node —
        // and MoveToDestination puts them where they belong once the cultivation is resolved, so
        // they never pass through the hand.
        if (target.Pile == null && destination == PileType.Hand)
        {
            await CardPileCmd.AddGeneratedCardToCombat(target, PileType.Hand, player, CardPilePosition.Top);
        }

        if (retain)
        {
            target.AddKeyword(CardKeyword.Retain);
        }

        List<CardModel> materials = HandMaterials(player, target);
        if (materials.Count == 0)
        {
            await MoveToDestination(choiceContext, player, target, destination);
            return false;
        }

        CultivateSession session = new(player, target, amount, doubleMain, source);
        CultivateSession.Active = session;
        CultivateOverlay.Show(session);

        bool stillOurs = true;
        IEnumerable<CardModel> picked;
        try
        {
            // No source: the main material is back in the hand by the time it is consumed below.
            picked = await CardSelectCmd.FromHand(
                choiceContext, player,
                new CardSelectorPrefs(MainPrompt, 1) { RequireManualConfirmation = true },
                card => materials.Contains(card), null!);
        }
        finally
        {
            // Only the session that still owns the shared overlay may tear it down: a cultivation
            // abandoned by a save & load can resume at any moment, and it must not clear the
            // session or the overlay of the cultivation the player is looking at right now.
            stillOurs = CultivateSession.Active == session;
            if (stillOurs)
            {
                CultivateSession.Active = null;
                CultivateOverlay.Dismiss();
            }
        }

        if (!stillOurs)
        {
            // The combat was rebuilt underneath this cultivation. The target belongs to the old
            // fight, and the freshly loaded save already has it wherever it should be, so moving it
            // anywhere now would duplicate it into a world that no longer exists.
            return false;
        }

        CardModel? main = picked.FirstOrDefault();
        if (main == null)
        {
            await MoveToDestination(choiceContext, player, target, destination);
            return false;
        }

        List<CardModel> subs = AutoFill(player, target, main, amount);
        CultivateBonus bonus = BuildBonus(target, main, subs, doubleMain);
        Apply(target, bonus);

        await ConsumeMaterial(choiceContext, player, main, true, target, doubleMain);
        foreach (CardModel card in subs)
        {
            await ConsumeMaterial(choiceContext, player, card, false, target, doubleMain);
        }

        await MoveToDestination(choiceContext, player, target, destination);
        return true;
    }

    private static async Task ConsumeMaterial(PlayerChoiceContext choiceContext, Player player, CardModel card,
        bool isMain, CardModel target, bool doubleMain)
    {
        if (card is IOnCultivated hook)
        {
            await hook.OnCultivated(choiceContext, player, isMain, target);
        }

        bool exhaust = IsExhaustMaterial(player, card) || (isMain && doubleMain);

        if (exhaust)
        {
            await CardCmd.Exhaust(choiceContext, card);

            // 土壤改造：净化 draws for every exhausted material.
            if (player.Creature.GetPower<SoilExhaustPower>() is { Amount: > 0 } soilExhaust)
            {
                await CardPileCmd.Draw(choiceContext, soilExhaust.Amount, player);
            }

            return;
        }

        // 土壤改造：回肥 keeps the main material usable this turn (and it does not dry out).
        if (isMain && player.Creature.GetPower<SoilBackPower>() is { Amount: > 0 } soilBack)
        {
            card.EnergyCost.AddThisTurnOrUntilPlayed(-soilBack.Amount, reduceOnly: true);
            return;
        }

        if (card is not IWontDry && !CultivateDryModifier.IsDry(card))
        {
            card.AddModifier<CultivateDryModifier>();
        }
    }

    private static async Task MoveToDestination(PlayerChoiceContext choiceContext, Player player, CardModel target,
        PileType destination)
    {
        if (target.Pile?.Type == destination)
        {
            return;
        }

        if (target.Pile == null)
        {
            // Placed into a pile for the first time: go through the generated-card entry point so
            // the combat history records it, then it lands at a random spot in the draw pile.
            await CardPileCmd.AddGeneratedCardToCombat(target, destination, player, CardPilePosition.Random);
            return;
        }

        await CardPileCmd.Add(target, destination, CardPilePosition.Random);
    }

}
