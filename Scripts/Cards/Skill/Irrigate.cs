using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>灌溉: solid block now, and a strong water nutrient later.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class Irrigate : AbstractMuelsyseCard, ICultivateNutrient
{
    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(12, ValueProp.Move),
        new DynamicVar("NutrientWater", 2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Nutrient),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water)
    ];

    public IEnumerable<CultivateCalculator> NutrientCalculators => [
        CultivateCalculator.Add(CultivateAim.Water, DynamicVars["NutrientWater"].IntValue)
    ];

    public Irrigate() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);
        DynamicVars["NutrientWater"].UpgradeValueBy(2);
    }
}
