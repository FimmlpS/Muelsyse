using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Muelsyse.Scripts.Cards.Status;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics.Stroll;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>在雪落处相见: a wide 漫步, and every card chosen leaves a unit of water behind.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class MeetInSnow : AbstractMuelsyseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Stroll", 8)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Stroll),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water),
        .. HoverTipFactory.FromCardWithCardHoverTips<Water>()
    ];

    public MeetInSnow() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await StrollCmd.Stroll(choiceContext, Owner, DynamicVars["Stroll"].IntValue, this,
            waterPerSelected: true);
    }

    protected override void OnUpgrade() => DynamicVars["Stroll"].UpgradeValueBy(2);
}
