using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>璀璨: waiting in the draw pile, it offers energy each time it arrives there.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class Resplendent : AbstractMuelsyseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];

    public Resplendent() : base(-1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType,
        AbstractModel? clonedBy)
    {
        if (card != this || Pile?.Type != PileType.Draw || oldPileType == PileType.Draw
            || CombatState == null || Owner.PlayerCombatState == null)
        {
            return;
        }

        PlayerChoiceContext choiceContext = new BlockingPlayerChoiceContext();
        int energy = DynamicVars.Energy.IntValue;

        if (CombatState.CurrentSide == CombatSide.Player)
        {
            await PlayerCmd.GainEnergy(energy, Owner);
        }
        else
        {
            await QueueEnergy(choiceContext);
        }
    }

    /// <summary>
    /// The draw pile is also filled at combat start, but the engine does that by moving the freshly
    /// cloned deck cards in directly (<c>Player.PopulateCombatState</c>), which never dispatches
    /// <see cref="AfterCardChangedPiles"/>. This hook runs right after that population, while the
    /// card still sits in the draw pile.
    /// </summary>
    public override async Task BeforeCombatStart()
    {
        if (Pile?.Type != PileType.Draw || CombatState == null || Owner.PlayerCombatState == null)
        {
            return;
        }

        // Energy granted now would be wiped: the turn starts with ResetEnergy (Energy = MaxEnergy),
        // which happens after this hook. EnergyNextTurnPower pays out right after that reset instead,
        // so the bonus lands on the opening turn.
        await QueueEnergy(new BlockingPlayerChoiceContext());
    }

    private async Task QueueEnergy(PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Apply<EnergyNextTurnPower>(
            choiceContext, Owner.Creature, DynamicVars.Energy.IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        RemoveKeyword(CardKeyword.Unplayable);
    }
}
