using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Enums;
using System.Runtime.CompilerServices;

namespace Muelsyse.Scripts.Mechanics;

/// <summary>
/// 协同 as a global keyword rule: any card that has the Together keyword has its damage/block
/// halved when the player uses it, and every living manifold then repeats the attack or block.
/// </summary>
public class TogetherListener : CustomSingletonModel
{
    private static readonly ConditionalWeakTable<CardModel, object> PreviewCards = new();
    private static readonly object PreviewMarker = new();

    public TogetherListener() : base(HookType.Combat)
    {
    }

    /// <summary>
    /// Marks a temporary card used only by the cultivation comparison screen. 协同 must not halve
    /// the numbers shown there: the previews display printed (cultivated) values instead.
    /// </summary>
    public static void MarkPreviewCard(CardModel card)
    {
        PreviewCards.AddOrUpdate(card, PreviewMarker);
    }

    private static bool IsPreviewCard(CardModel? card)
    {
        return card != null && PreviewCards.TryGetValue(card, out _);
    }

    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (amount <= 0m || IsPreviewCard(cardSource) || !IsTogetherAttack(cardSource, dealer, props))
        {
            return 0m;
        }

        // Halve the card's printed damage, but never reduce a hit below 1. The clamp matters for
        // multi-hit cards whose later hits use a smaller value (铃兰's block-based second hit).
        decimal half = Math.Floor(AdjustedDamage(cardSource!, props) / 2m);
        return -Math.Min(half, Math.Max(0m, amount - 1m));
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

        // Every manifold repeats the card's current displayed damage (cultivation, strength and any
        // other card-damage buffs already baked in). The card is deliberately not passed as the
        // damage source: the pipeline would otherwise apply those same card modifiers a second time.
        // Target-side effects such as 易伤 still run once, as they do for the player's hit.
        decimal amount = DisplayDamage(card);
        if (amount <= 0m)
        {
            return;
        }

        ValueProp props = Props(card);

        foreach (List<DamageResult> hit in command.Results)
        {
            List<Creature> targets = DamageableTargets(hit.Select(result => result.Receiver));
            if (targets.Count == 0)
            {
                continue;
            }

            foreach (Creature manifold in ManifoldCmd.Alive(player))
            {
                if (CombatManager.Instance.IsOverOrEnding)
                {
                    return;
                }

                // The card's own hit, or the previous manifold's swing, can take a target out of the
                // fight between one manifold and the next. Re-filter before every swing instead of
                // reusing the list the manifold before us attacked.
                targets = DamageableTargets(targets);
                if (targets.Count == 0)
                {
                    break;
                }

                if (!manifold.IsAlive || manifold.CombatState == null)
                {
                    continue;
                }

                await CreatureCmd.TriggerAnim(manifold, "Attack", 0.15f);
                await CreatureCmd.Damage(choiceContext, targets, amount, props, manifold, null, null);
                foreach (Creature target in targets)
                {
                    await ManifoldCmd.TrySteal(choiceContext, manifold, target);
                }
            }
        }
    }

    /// <summary>
    /// The creatures a manifold may still hit. Being alive is not enough: a creature can stay alive
    /// after leaving combat (it escaped, or it was swept off the field when the primary enemy died),
    /// and <c>CreatureCmd.Damage</c> takes the combat state from the first target of the list and
    /// then throws on it inside the combat history. That is exactly the 举星 crash: the attack kills
    /// the enemy it is aimed at and the surviving minions are no longer in combat.
    /// </summary>
    private static List<Creature> DamageableTargets(IEnumerable<Creature> creatures)
    {
        return creatures
            .Where(target => target.IsAlive && target.CombatState?.IsLiveCombat() == true)
            .Distinct()
            .ToList();
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

    /// <summary>
    /// 协同 buff/debuff cards hand out half as many stacks, rounded up (3 becomes 2), and every
    /// manifold then applies that same amount in turn (a buff to itself, a debuff to the same
    /// target). The additive hook takes a delta, so subtracting floor(amount / 2) leaves
    /// ceil(amount / 2).
    /// </summary>
    public override decimal ModifyPowerAmountGivenAdditive(PowerModel power, Creature giver, decimal amount,
        Creature? target, CardModel? cardSource)
    {
        if (amount <= 0m || power.Type == PowerType.None || !IsTogetherPower(cardSource, giver))
        {
            return 0m;
        }

        return -Math.Floor(amount / 2m);
    }

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (amount <= 0m || power.Type == PowerType.None || !IsTogetherPower(cardSource, applier))
        {
            return;
        }

        Creature owner = power.Owner;
        if (owner == null || cardSource!.Owner is not { } player)
        {
            return;
        }

        foreach (Creature manifold in ManifoldCmd.Alive(player))
        {
            if (!manifold.IsAlive)
            {
                continue;
            }

            Creature destination = power.Type == PowerType.Debuff ? owner : manifold;
            await PowerCmd.Apply(choiceContext, (PowerModel)power.MutableClone(), destination, amount, manifold,
                cardSource, silent: true);
        }
    }

    private static bool IsTogetherPower(CardModel? cardSource, Creature? giver)
    {
        return cardSource is ITogetherPowerCard
               && giver != null
               && giver == cardSource.Owner?.Creature
               && cardSource.Keywords.Contains(MuelsyseKeywords.Together);
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

    /// <summary>The card's own damage, before cultivation and enchantments are added.</summary>
    private static decimal PrintedDamage(CardModel card)
    {
        decimal value = BaseValue(card, "Damage");
        if (value == 0m && card.DynamicVars.TryGetValue("CalculatedDamage", out DynamicVar? calculated)
            && calculated is CalculatedVar calculable)
        {
            value = calculable.Calculate(null);
        }

        return value;
    }

    private static decimal AdjustedDamage(CardModel card, ValueProp props)
    {
        decimal value = PrintedDamage(card);
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

    /// <summary>
    /// The damage the card currently shows in combat: its printed value after cultivation, 协同's
    /// halving and every other card-damage buff (strength and the like). Target-specific effects
    /// such as 易伤 are excluded, exactly like the in-hand preview without a hovered enemy; the real
    /// hit applies them once. This is the number each manifold repeats.
    /// </summary>
    public static decimal DisplayDamage(CardModel card)
    {
        ValueProp props = Props(card);
        if (card.Owner is { } owner && card.CombatState is { } combatState)
        {
            return Hook.ModifyDamage(owner.RunState, combatState, null, owner.Creature,
                PrintedDamage(card), props, card, null, ModifyDamageHookType.All,
                CardPreviewMode.None, out _);
        }

        return AdjustedDamage(card, props);
    }

    public static ValueProp Props(CardModel card)
    {
        return card.DynamicVars.TryGetValue("Damage", out DynamicVar? value) && value is DamageVar damageVar
            ? damageVar.Props
            : ValueProp.Move;
    }

    /// <summary>The attack each manifold adds when following this card, shown above their heads.</summary>
    public static decimal DamageBonus(CardModel card)
    {
        return DisplayDamage(card);
    }

    /// <summary>The 协同 attack card the player is hovering, selecting or playing right now, if any.</summary>
    public static CardModel? CurrentTogetherCard()
    {
        NPlayerHand? hand = NPlayerHand.Instance;
        CardModel? candidate = hand?.FocusedHolder?.CardModel;

        // A card that has been picked up is in the targeting phase, before an enemy is designated.
        // Its NPlayerHand holder is no longer focused, so read the active NCardPlay instead.
        if (candidate == null && hand != null)
        {
            candidate = hand.GetChildren().OfType<NCardPlay>()
                .Select(play => play.Holder?.CardModel)
                .FirstOrDefault(card => card != null);
        }

        if (candidate == null
            && hand?.GetNodeOrNull<NSelectedHandCardContainer>("%SelectedHandCardContainer")
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
