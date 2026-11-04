using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Muelsyse.Scripts.Cards.Attack;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>种一颗星星: refills the hand, then plants a cultivated 举星 in the draw pile.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class PlantAStar : AbstractMuelsyseCard
{
    private const int FullHandSize = 10;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Cultivate", 6)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Cultivate),
        .. HoverTipFactory.FromCardWithCardHoverTips<LiftStar>()
    ];

    public PlantAStar() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int missing = Math.Max(0, FullHandSize - PileType.Hand.GetPile(Owner).Cards.Count);
        if (missing > 0)
        {
            await CardPileCmd.Draw(choiceContext, missing, Owner);
        }

        if (CombatState is not { } combatState)
        {
            return;
        }

        LiftStar liftStar = combatState.CreateCard<LiftStar>(Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(liftStar);
        }

        await CultivateCmd.Cultivate(choiceContext, Owner, liftStar,
            DynamicVars["Cultivate"].IntValue, this, destination: PileType.Draw);
    }

    protected override void OnUpgrade()
    {
    }
}
