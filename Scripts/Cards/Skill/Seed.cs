using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics.Stroll;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>播种: 漫步, planting pollen on the first card the player picks.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class Seed : AbstractMuelsyseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Pollen", 1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Stroll),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Pollen)
    ];

    public Seed() : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await StrollCmd.Stroll(choiceContext, Owner, 3, this,
            firstPollen: DynamicVars["Pollen"].IntValue);
    }

    protected override void OnUpgrade() => DynamicVars["Pollen"].UpgradeValueBy(1);
}
