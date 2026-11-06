using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Muelsyse.Scripts.Cards.Status;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Powers;

namespace Muelsyse.Scripts.Cards.Power;

/// <summary>生命之源: adds upgraded 水 to the hand every turn.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class SourceOfLife : AbstractMuelsyseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Water", 1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water),
        // The power always hands out upgraded 水, upgraded card or not.
        .. HoverTipFactory.FromCardWithCardHoverTips<Water>(upgrade: true)
    ];

    public SourceOfLife() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SourceOfLifePower>(choiceContext, Owner.Creature,
            DynamicVars["Water"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Water"].UpgradeValueBy(1);
}
