using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics.Stroll;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>逆时性焓变: 漫步, then draw.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class InverseChange : AbstractMuelsyseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Draw", 2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Stroll)
    ];

    public InverseChange() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await StrollCmd.Stroll(choiceContext, Owner, IsUpgraded ? 5 : 3, this);
        await CardPileCmd.Draw(choiceContext, DynamicVars["Draw"].IntValue, Owner);
    }

    protected override void OnUpgrade() => DynamicVars["Draw"].UpgradeValueBy(1);
}
