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

/// <summary>精调: cheap energy and a strong, scaling nutrient.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class FineBlend : AbstractMuelsyseCard, ICultivateNutrient
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Nutrient)
    ];

    public IEnumerable<CultivateCalculator> NutrientCalculators => [
        CultivateCalculator.Add(CultivateAim.Cost, 1),
        CultivateCalculator.Multiply(CultivateAim.Attack, IsUpgraded ? 1.75m : 1.5m),
        CultivateCalculator.Multiply(CultivateAim.Block, IsUpgraded ? 1.75m : 1.5m)
    ];

    public FineBlend() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
    }

    protected override void OnUpgrade() => DynamicVars.Energy.UpgradeValueBy(1);
}
