using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Enums;

namespace Muelsyse.Scripts.Mechanics;

/// <summary>
/// 协同 as a global keyword rule: any card that has the Together keyword has its damage/block
/// halved when the player uses it, and every living manifold then repeats the attack or block.
/// </summary>
public class TogetherListener : CustomSingletonModel
{
    public TogetherListener() : base(HookType.Combat)
    {
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (!IsTogetherAttack(cardSource, dealer, props))
        {
            return 0m;
        }

        return -Math.Floor(AdjustedDamage(cardSource!, props) / 2m);
    }

    public override async Task AfterAttack(PlayerChoiceContext choiceContext, AttackCommand command)
    {
        if (command.Attacker is not { } attacker
            || command.ModelSource is not CardModel card
            || !IsTogetherAttack(card, attacker, command.DamageProps)
            || card.Owner is not { } player)
        {
            return;
        }

        decimal amount = Halve(AdjustedDamage(card, command.DamageProps));
        if (amount <= 0m)
        {
            return;
        }

        foreach (List<DamageResult> hit in command.Results)
        {
            List<Creature> targets = hit
                .Select(result => result.Receiver)
                .Distinct()
                .Where(target => target.IsAlive)
                .ToList();
            if (targets.Count == 0)
            {
                continue;
            }

            foreach (Creature manifold in ManifoldCmd.Alive(player))
            {
                if (!manifold.IsAlive)
                {
                    continue;
                }

                await CreatureCmd.TriggerAnim(manifold, "Attack", 0.15f);
                await CreatureCmd.Damage(choiceContext, targets, amount, ValueProp.Move, manifold, card,
                    command.CardPlay);
                foreach (Creature target in targets)
                {
                    await ManifoldCmd.TrySteal(choiceContext, manifold, target);
                }
            }
        }
    }

    public override async Task AfterBlockGained(Creature creature, decimal amount, ValueProp props,
        CardModel? cardSource)
    {
        if (amount <= 0m
            || !IsTogetherBlock(cardSource, creature)
            || cardSource!.Owner is not { } player)
        {
            return;
        }

        foreach (Creature manifold in ManifoldCmd.Alive(player))
        {
            int threshold = ManifoldCmd.Threshold(manifold);
            if (threshold <= 0)
            {
                continue;
            }

            decimal block = Math.Min(amount, threshold);
            if (block > 0m)
            {
                await CreatureCmd.GainBlock(manifold, block, ValueProp.Move, null, fast: true);
            }
        }
    }

    private static bool IsTogetherAttack(CardModel? cardSource, Creature? dealer, ValueProp props)
    {
        return cardSource != null
               && dealer != null
               && dealer == cardSource.Owner?.Creature
               && cardSource.Keywords.Contains(MuelsyseKeywords.Together)
               && props.HasFlag(ValueProp.Move)
               && !props.HasFlag(ValueProp.Unpowered);
    }

    private static bool IsTogetherBlock(CardModel? cardSource, Creature target)
    {
        return cardSource != null
               && target == cardSource.Owner?.Creature
               && cardSource.Keywords.Contains(MuelsyseKeywords.Together);
    }

    private static decimal BaseValue(CardModel card, string key)
    {
        return card.DynamicVars.TryGetValue(key, out DynamicVar? value) ? value.BaseValue : 0m;
    }

    private static decimal AdjustedDamage(CardModel card, ValueProp props)
    {
        decimal value = BaseValue(card, "Damage");
        if (card.Enchantment is not { } enchantment)
        {
            return value;
        }

        value += enchantment.EnchantDamageAdditive(value, props);
        return value * enchantment.EnchantDamageMultiplicative(value, props);
    }

    /// <summary>Half of the card's value, rounded up so that 3 becomes 2 (matching the printed 协同 values).</summary>
    public static decimal Halve(decimal value)
    {
        return value - Math.Floor(value / 2m);
    }

    /// <summary>The attack each manifold adds when following this card, shown above their heads.</summary>
    public static decimal DamageBonus(CardModel card)
    {
        ValueProp props = card.DynamicVars.TryGetValue("Damage", out DynamicVar? value)
                          && value is DamageVar damageVar
            ? damageVar.Props
            : ValueProp.Move;
        return Halve(AdjustedDamage(card, props));
    }

    /// <summary>The 协同 attack card the player is hovering, selecting or playing right now, if any.</summary>
    public static CardModel? CurrentTogetherCard()
    {
        CardModel? candidate = NPlayerHand.Instance?.FocusedHolder?.CardModel;

        if (candidate == null
            && NPlayerHand.Instance?.GetNodeOrNull<NSelectedHandCardContainer>("%SelectedHandCardContainer")
                is { } selected)
        {
            candidate = selected.GetChildren().OfType<NSelectedHandCardHolder>()
                .Select(holder => holder.CardModel)
                .FirstOrDefault(card => card != null);
        }

        if (candidate == null && NCardPlayQueue.Instance is { } queue)
        {
            candidate = queue.GetChildren().OfType<NCard>()
                .Select(node => node.Model)
                .FirstOrDefault(card => card != null);
        }

        return candidate != null && IsTogetherAttackCard(candidate) ? candidate : null;
    }

    public static bool IsTogetherAttackCard(CardModel card)
    {
        return card.Type == CardType.Attack && card.Keywords.Contains(MuelsyseKeywords.Together);
    }
}
