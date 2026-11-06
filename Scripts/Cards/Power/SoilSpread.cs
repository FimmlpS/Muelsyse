using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Powers;

namespace Muelsyse.Scripts.Cards.Power;

/// <summary>土壤改造：风花: every cultivation also plants 散孢 pollen on the target.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class SoilSpread : AbstractMuelsyseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Spread", 2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Cultivate),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Pollen)
    ];

    public SoilSpread() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SoilSpreadPower>(choiceContext, Owner.Creature,
            DynamicVars["Spread"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Spread"].UpgradeValueBy(1);
}
