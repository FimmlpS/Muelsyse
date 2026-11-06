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

/// <summary>不定形态: 漫步 at the start of every turn.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class UncertainForm : AbstractMuelsyseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Stroll", 4)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Stroll)
    ];

    public UncertainForm() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<UncertainFormPower>(choiceContext, Owner.Creature,
            DynamicVars["Stroll"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Stroll"].UpgradeValueBy(2);
}
