using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics.Stroll;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>绿野幻梦: 漫步 one card, plus one for every card already played this turn.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class DorothyVision : AbstractMuelsyseCard
{
    private const string CountKey = "Count";

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(1m),
        new CalculationExtraVar(1m),
        new CalculatedVar(CountKey).WithMultiplier(PlayedThisTurn)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Stroll)
    ];

    public DorothyVision() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    private static decimal PlayedThisTurn(CardModel card, Creature? target)
    {
        return CombatManager.Instance.History.CardPlaysStarted.Count(entry =>
            entry.HappenedThisTurn(card.CombatState)
            && entry.CardPlay.Player == card.Owner
            && entry.CardPlay.Card != card);
    }

    private int Count => Math.Max(0, (int)((CalculatedVar)DynamicVars[CountKey]).Calculate(null));

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await StrollCmd.Stroll(choiceContext, Owner, Count, this);
    }

    protected override void OnUpgrade() => DynamicVars.CalculationBase.UpgradeValueBy(2);
}
