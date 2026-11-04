using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>终末的岁月: exchanges hand and discard, then cultivates a 紫颂.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class EndTime : AbstractMuelsyseCard
{
    private const int DiscardLimit = 10;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Cultivate", 1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Cultivate),
        .. HoverTipFactory.FromCardWithCardHoverTips<ChorusFlower>()
    ];

    public EndTime() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.PlayerCombatState is not { } combatState)
        {
            return;
        }

        List<CardModel> hand = combatState.Hand.Cards.ToList();
        List<CardModel> discard = combatState.DiscardPile.Cards.ToList();

        if (discard.Count < DiscardLimit)
        {
            await CardPileCmd.Add(hand, PileType.Discard);
            await CardPileCmd.Add(discard, PileType.Hand);
        }

        if (CombatState is not { } combat)
        {
            return;
        }

        ChorusFlower chorusFlower = combat.CreateCard<ChorusFlower>(Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(chorusFlower);
        }

        await CultivateCmd.Cultivate(choiceContext, Owner, chorusFlower,
            DynamicVars["Cultivate"].IntValue, this);
    }

    protected override void OnUpgrade()
    {
    }
}
