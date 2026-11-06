using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>生枝: cultivates a card in hand and returns it there.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class BornShoots : AbstractMuelsyseCard, ICultivateCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Cultivate", 1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Cultivate)
    ];

    public BornShoots() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel? target = await CultivateCmd.SelectTarget(choiceContext, Owner);
        if (target == null)
        {
            return;
        }

        await CultivateCmd.Cultivate(choiceContext, Owner, target,
            DynamicVars["Cultivate"].IntValue, this);
    }

    protected override void OnUpgrade() => DynamicVars["Cultivate"].UpgradeValueBy(1);
}
