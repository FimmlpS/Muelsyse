using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Powers;

namespace Muelsyse.Scripts.Relics;

/// <summary>
/// 生命循环: 润化 is stored here. Every 20 stacks trigger 溯源 on the basic manifold;
/// every 16 stacks spawn a short-lived extra manifold.
/// </summary>
[Pool(typeof(MuelsyseRelicPool))]
public class LifeCircle : AbstractMuelsyseRelic
{
    public const int StartingWater = 4;
    public const int CopyCost = 16;
    public const int SourceCost = 20;
    public const int SourceMaxHpGain = 20;
    public const int SourceMultiplierGain = 5;
    public const int ExtraLifespan = 3;

    private int _counter;

    public override RelicRarity Rarity => RelicRarity.Starter;

    public override bool ShowCounter => true;

    public override int DisplayAmount => _counter;

    public override async Task BeforeCombatStart()
    {
        _counter = StartingWater;
        InvokeDisplayAmountChanged();

        if (Owner.Creature.CombatState != null)
        {
            await ManifoldCmd.EnsureLink(new ThrowingPlayerChoiceContext(), Owner);
        }
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner)
        {
            return;
        }

        if (ManifoldCmd.Basic(player) == null)
        {
            await ManifoldCmd.SpawnBasic(choiceContext, player);
        }
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner != Owner)
        {
            return;
        }

        await AddWater(choiceContext, Owner, 1);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(Owner.Creature))
        {
            return;
        }

        int energy = Owner.PlayerCombatState?.Energy ?? 0;
        int maxEnergy = Owner.PlayerCombatState?.MaxEnergy ?? 0;
        int converted = Math.Min(energy, maxEnergy) * 2;
        if (converted > 0)
        {
            await AddWater(choiceContext, Owner, converted);
        }
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _counter = 0;
        InvokeDisplayAmountChanged();
        return Task.CompletedTask;
    }

    public static async Task AddWater(PlayerChoiceContext choiceContext, Player player, int amount,
        CardModel? cardSource = null)
    {
        LifeCircle? relic = player.Relics.OfType<LifeCircle>().FirstOrDefault();
        if (relic == null || amount <= 0 || player.Creature.CombatState == null)
        {
            return;
        }

        relic._counter += amount;
        relic.InvokeDisplayAmountChanged();
        await relic.Resolve(choiceContext, player, cardSource);
    }

    private async Task Resolve(PlayerChoiceContext choiceContext, Player player, CardModel? cardSource)
    {
        bool changed = false;

        while (_counter >= SourceCost)
        {
            _counter -= SourceCost;
            changed = true;
            await ResolveSource(choiceContext, player, cardSource);
        }

        while (_counter >= CopyCost &&
               ManifoldCmd.Extras(player).Count(pet => pet.IsAlive) < ManifoldCmd.MaxRelicCopies)
        {
            _counter -= CopyCost;
            changed = true;
            await ManifoldCmd.SpawnExtra(choiceContext, player, ExtraLifespan, cardSource);
        }

        if (changed)
        {
            Flash();
        }

        InvokeDisplayAmountChanged();
    }

    private async Task ResolveSource(PlayerChoiceContext choiceContext, Player player, CardModel? cardSource)
    {
        Creature basic = await ManifoldCmd.SpawnBasic(choiceContext, player, cardSource);
        await CreatureCmd.GainMaxHp(basic, SourceMaxHpGain);
        await CreatureCmd.Heal(basic, SourceMaxHpGain);
        await PowerCmd.Apply<SourcePower>(choiceContext, basic, SourceMultiplierGain, player.Creature, cardSource);

        ManifoldLinkPower? link = player.Creature.GetPower<ManifoldLinkPower>();
        if (link != null)
        {
            link.SourceMultiplier = basic.GetPower<SourcePower>()?.Amount ?? SourceMultiplierGain;
            link.RefreshDisplay();
        }
    }
}
