using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Powers;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>芳兰: raises the threshold of every living manifold.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class Orchid : AbstractMuelsyseCard, ICultivateNutrient
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Threshold", 1),
        new DynamicVar("NutrientBlock", 4)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Threshold),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Nutrient)
    ];

    public IEnumerable<CultivateCalculator> NutrientCalculators => [
        CultivateCalculator.Add(CultivateAim.Attack, -2),
        CultivateCalculator.Add(CultivateAim.Block, DynamicVars["NutrientBlock"].IntValue)
    ];

    public Orchid() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        Creature? basic = ManifoldCmd.Basic(Owner);
        if (basic != null)
        {
            await PowerCmd.Apply<ManifoldPower>(choiceContext, basic,
                DynamicVars["Threshold"].IntValue, Owner.Creature, this);
        }

        foreach (Creature manifold in ManifoldCmd.Extras(Owner))
        {
            if (manifold.IsAlive)
            {
                await PowerCmd.Apply<ManifoldPower>(choiceContext, manifold,
                    DynamicVars["Threshold"].IntValue, Owner.Creature, this);
            }
        }
    }

    protected override void OnUpgrade() => DynamicVars["NutrientBlock"].UpgradeValueBy(4);
}
