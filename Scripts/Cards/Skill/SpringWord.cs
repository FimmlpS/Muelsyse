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

/// <summary>春语: cultivates a 铃兰, doubling the main material.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class SpringWord : AbstractMuelsyseCard, ICultivateCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Cultivate", 1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Cultivate),
        .. HoverTipFactory.FromCardWithCardHoverTips<Suzuran>(IsUpgraded)
    ];

    public SpringWord() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combatState)
        {
            return;
        }

        Suzuran suzuran = combatState.CreateCard<Suzuran>(Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(suzuran);
        }

        await CultivateCmd.Cultivate(choiceContext, Owner, suzuran,
            DynamicVars["Cultivate"].IntValue, this, doubleMain: true);
    }

    protected override void OnUpgrade()
    {
    }
}
