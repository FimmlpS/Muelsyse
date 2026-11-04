using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>化梦泡影: gathers every manifold's block onto the player.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class DreamBubble : AbstractMuelsyseCard, ICultivateNutrient
{
    public override bool GainsBlock => true;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Nutrient),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Threshold)
    ];

    public IEnumerable<CultivateCalculator> NutrientCalculators => [
        CultivateCalculator.Multiply(CultivateAim.Block, IsUpgraded ? 2m : 1.5m)
    ];

    public DreamBubble() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int total = 0;
        foreach (Creature manifold in ManifoldCmd.Alive(Owner))
        {
            int block = manifold.Block;
            if (block <= 0)
            {
                continue;
            }

            total += block;
            await CreatureCmd.LoseBlock(choiceContext, manifold, block, Owner.Creature);
        }

        if (total > 0)
        {
            await CreatureCmd.GainBlock(Owner.Creature, total, ValueProp.Move, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
