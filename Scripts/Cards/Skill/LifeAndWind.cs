using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>生命与风: a dandelion arrives already carrying pollen, then gets cultivated.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class LifeAndWind : AbstractMuelsyseCard, ICultivateCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Pollen", 2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Cultivate),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Pollen),
        .. HoverTipFactory.FromCardWithCardHoverTips<Dandelion>(IsUpgraded)
    ];

    public LifeAndWind() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (CombatState is not { } combatState)
        {
            return;
        }

        int amount = DynamicVars["Pollen"].IntValue;

        Dandelion dandelion = combatState.CreateCard<Dandelion>(Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(dandelion);
        }

        PollenCmd.PlantRandom(dandelion, amount);
        await CultivateCmd.Cultivate(choiceContext, Owner, dandelion, amount, this);
    }

    protected override void OnUpgrade() => DynamicVars["Pollen"].UpgradeValueBy(1);
}
