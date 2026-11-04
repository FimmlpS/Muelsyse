using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Cards.Skill;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Relics;

namespace Muelsyse.Scripts.Powers;

/// <summary>
/// Links the player to their manifolds: damage that goes through the player's block is absorbed by
/// manifolds, and the relic's 润化 engine listens through this power.
/// </summary>
public class ManifoldLinkPower : AbstractMuelsysePower
{
    public int ThresholdBonus { get; set; }

    public int HpBonus { get; set; }

    public int SourceMultiplier { get; set; } = 1;

    public int StrengthBonus { get; set; }

    public int CleanWaterAmount { get; set; }

    private readonly List<(Creature Manifold, decimal Amount)> _pending = [];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => Owner.Player == null ? 0 : ManifoldCmd.TotalThreshold(Owner.Player);

    public void RefreshDisplay()
    {
        InvokeDisplayAmountChanged();
    }

    /// <summary>Keeps the displayed total threshold in sync when a manifold's level or 溯源 changes.</summary>
    public override Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power,
        decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power is ManifoldPower or SourcePower)
        {
            RefreshDisplay();
        }

        return Task.CompletedTask;
    }

    public override decimal ModifyHpLostBeforeOsty(Creature target, decimal amount, ValueProp props,
        Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || !props.IsPoweredAttack())
        {
            return amount;
        }

        _pending.Clear();
        Player? player = Owner.Player;
        if (player == null || amount <= 0)
        {
            return amount;
        }

        decimal remaining = amount;
        foreach (Creature manifold in ManifoldCmd.Extras(player).Where(m => m.IsAlive))
        {
            remaining = Absorb(manifold, remaining);
            if (remaining <= 0)
            {
                break;
            }
        }

        Creature? basic = ManifoldCmd.Basic(player);
        if (remaining > 0 && basic is { IsAlive: true })
        {
            remaining = Absorb(basic, remaining);
        }

        if (_pending.Count > 0)
        {
            Flash();
        }

        return remaining;
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || _pending.Count == 0)
        {
            return;
        }

        List<(Creature Manifold, decimal Amount)> plan = [.._pending];
        _pending.Clear();

        foreach ((Creature manifold, decimal amount) in plan)
        {
            if (!manifold.IsAlive || amount <= 0)
            {
                continue;
            }

            await DamageManifold(choiceContext, manifold, amount, dealer, cardSource);
        }

        InvokeDisplayAmountChanged();
    }

    /// <summary>
    /// Damages a manifold with its own block resolved first, then its HP.
    /// <para>
    /// <see cref="CreatureCmd.Damage"/> charges a pet's damage to <c>target.PetOwner.Creature</c>'s
    /// block (see its <c>DamageBlockInternal</c> call), so a plain damage call would spend the
    /// player's block a second time and ignore the manifold's own block entirely. The manifold's
    /// block is therefore spent here by hand, and the remainder is routed through the damage
    /// pipeline as <see cref="ValueProp.Unblockable"/> so the engine does not take the player's
    /// block for it.
    /// </para>
    /// </summary>
    private static async Task DamageManifold(PlayerChoiceContext choiceContext, Creature manifold, decimal amount,
        Creature? dealer, CardModel? cardSource)
    {
        if (manifold.Block > 0)
        {
            decimal blocked = Math.Min(manifold.Block, amount);
            await CreatureCmd.LoseBlock(choiceContext, manifold, blocked, dealer);
            amount -= blocked;
        }

        if (amount <= 0)
        {
            return;
        }

        await CreatureCmd.Damage(choiceContext, manifold, amount,
            ValueProp.Move | ValueProp.Unpowered | ValueProp.Unblockable, dealer, cardSource, null);
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Player? player = Owner.Player;
        if (player == null || cardPlay.Card.Owner != player)
        {
            return;
        }

        int water = player.PlayerCombatState?.Hand.Cards.OfType<Wave>().Sum(card => card.WaterAmount) ?? 0;
        if (water > 0)
        {
            await LifeCircle.AddWater(choiceContext, player, water);
        }
    }

    private decimal Absorb(Creature manifold, decimal remaining)
    {
        int threshold = ManifoldCmd.Threshold(manifold);
        if (threshold <= 0)
        {
            return remaining;
        }

        decimal absorbed = Math.Min(remaining, threshold);
        _pending.Add((manifold, absorbed));
        return remaining - absorbed;
    }
}
