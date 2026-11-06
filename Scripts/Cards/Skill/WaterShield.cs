using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Cards.Status;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>水障壁: consumes up to four 水, taken from draw, then hand, then discard, for block.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class WaterShield : AbstractMuelsyseCard
{
    private const int MaxWater = 4;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(5, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water),
        .. HoverTipFactory.FromCardWithCardHoverTips<Water>()
    ];

    public WaterShield() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardModel> waters = WaterCmd.InConsumptionOrder(Owner, MaxWater);

        for (int i = 0; i < waters.Count; i++)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }

        foreach (CardModel water in waters)
        {
            await CardCmd.Exhaust(choiceContext, water);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);
}
