using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>复合型养料: restores every dried-out card in hand, then feeds card draw.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class CompositeNutrient : AbstractMuelsyseCard, ICultivateNutrient
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("NutrientDraw", 2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Nutrient),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Dry)
    ];

    public IEnumerable<CultivateCalculator> NutrientCalculators => [
        CultivateCalculator.Add(CultivateAim.Draw, DynamicVars["NutrientDraw"].IntValue)
    ];

    public CompositeNutrient() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.PlayerCombatState is not { } combatState)
        {
            return Task.CompletedTask;
        }

        foreach (CardModel card in combatState.Hand.Cards.ToList())
        {
            CultivateCmd.RemoveDry(card);
        }

        if (IsUpgraded)
        {
            CultivateCmd.RemoveDry(this);
        }

        return Task.CompletedTask;
    }

    protected override void OnUpgrade() => DynamicVars["NutrientDraw"].UpgradeValueBy(1);
}
