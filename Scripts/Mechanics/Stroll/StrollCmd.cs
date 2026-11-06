using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Powers;

namespace Muelsyse.Scripts.Mechanics.Stroll;

/// <summary>
/// 漫步: review up to <c>count</c> random cards of the discard pile (plus the free random cards 遗尘
/// adds), then shuffle any number of them back into the draw pile.
/// </summary>
public static class StrollCmd
{
    public static readonly LocString SelectPrompt = new("card_selection", "STROLL_SELECT");

    public static async Task Stroll(PlayerChoiceContext choiceContext, Player player, int count,
        AbstractModel? source = null, int firstPollen = 0, bool playFirst = false,
        StrollRemain remain = StrollRemain.None, bool waterPerSelected = false)
    {
        if (player.PlayerCombatState is not { } state || CombatManager.Instance.IsOverOrEnding)
        {
            return;
        }

        List<CardModel> options = [];
        List<CardModel> dust = [];

        List<CardModel> discard = state.DiscardPile.Cards.ToList();
        int samples = Math.Clamp(count, 0, discard.Count);
        for (int i = 0; i < samples; i++)
        {
            CardModel? card = player.RunState.Rng.CombatCardSelection.NextItem(discard);
            if (card == null)
            {
                break;
            }

            discard.Remove(card);
            options.Add(card);
        }

        // 遗尘: each stack puts a free random card into the selection. Unlike the discarded cards it
        // belongs to no pile; it only becomes a real card if the player picks it.
        if (player.Creature.GetPower<DustPower>() is { Amount: > 0 } dustPower)
        {
            for (int i = 0; i < dustPower.Amount; i++)
            {
                CardModel? card = CreateDustCard(player);
                if (card == null)
                {
                    continue;
                }

                card.SetToFreeThisCombat();
                RemainDustMarker.Mark(card);
                options.Add(card);
                dust.Add(card);
            }
        }

        if (options.Count == 0)
        {
            return;
        }

        List<CardModel> picked = (await CardSelectCmd.FromSimpleGrid(
            choiceContext, options, player,
            new CardSelectorPrefs(SelectPrompt, 0, options.Count))).ToList();

        HashSet<CardModel> chosen = new(picked);
        bool first = true;

        foreach (CardModel card in picked)
        {
            bool isFirst = first;
            first = false;

            // The card becomes a normal card as soon as it is picked; the badge only exists to warn
            // the player during this selection.
            RemainDustMarker.Clear(card);

            await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Random);

            // 苏生: every card that a 漫步 sends into the draw pile grants its block.
            await RevivePower.OnCardEnteredDraw(choiceContext, player);

            if (waterPerSelected)
            {
                await WaterCmd.AddToDiscard(choiceContext, player);
            }

            if (isFirst)
            {
                if (firstPollen > 0)
                {
                    PollenCmd.PlantRandom(card, firstPollen);
                }

                if (playFirst)
                {
                    await PlayCopy(choiceContext, card);
                }
            }

            if (card is IOnStrolled hook)
            {
                await hook.OnStrolled(choiceContext, player, isFirst);
            }
        }

        // Unchosen dust cards were illusions only. They never joined a pile, so there is nothing to
        // exhaust or discard: destroy them outright.
        foreach (CardModel card in dust.Where(card => !chosen.Contains(card)))
        {
            RemainDustMarker.Clear(card);
            card.CombatState?.RemoveCard(card);
        }

        List<CardModel> remaining = options
            .Where(card => !chosen.Contains(card) && !dust.Contains(card))
            .ToList();

        switch (remain)
        {
            case StrollRemain.Hand:
                foreach (CardModel card in remaining)
                {
                    if (HandLimit.IsFull(player))
                    {
                        break;
                    }

                    await CardPileCmd.Add(card, PileType.Hand);
                }

                break;

            case StrollRemain.ExhaustAndDraw:
                foreach (CardModel card in remaining)
                {
                    if (CombatManager.Instance.IsOverOrEnding)
                    {
                        break;
                    }

                    await CardCmd.Exhaust(choiceContext, card);
                    await CardPileCmd.Draw(choiceContext, 1, player);
                }

                break;
        }
    }

    /// <summary>Plays a throwaway copy of a card and removes it again once it resolves.</summary>
    public static async Task PlayCopy(PlayerChoiceContext choiceContext, CardModel card)
    {
        CardModel copy = card.CreateClone();
        await CardCmd.AutoPlay(choiceContext, copy, null);

        if (copy.Pile is { IsCombatPile: true })
        {
            await CardPileCmd.RemoveFromCombat(copy, skipVisuals: true);
        }
    }

    /// <summary>
    /// A random ordinary card for 遗尘. <see cref="CardFactory.FilterForCombat"/> alone is too broad:
    /// it lets tokens and statuses through, so the pool is restricted to real collectible cards.
    /// </summary>
    private static CardModel? CreateDustCard(Player player)
    {
        List<CardModel> pool = ModelDb.AllCards
            .Where(card => card.CanBeGeneratedInCombat
                           && card.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare
                           && card.Type is CardType.Attack or CardType.Skill or CardType.Power)
            .ToList();

        if (pool.Count == 0)
        {
            return null;
        }

        return CardFactory
            .GetForCombat(player, pool, 1, player.RunState.Rng.CombatCardGeneration)
            .FirstOrDefault();
    }
}
