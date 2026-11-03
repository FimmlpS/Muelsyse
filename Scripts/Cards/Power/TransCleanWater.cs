using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Powers;

namespace Muelsyse.Scripts.Cards.Power;

[Pool(typeof(MuelsyseCardPool))]
public class TransCleanWater : AbstractMuelsyseCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Absorb)
    ];

    public TransCleanWater() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ManifoldLinkPower link = await ManifoldCmd.EnsureLink(choiceContext, Owner);
        link.CleanWaterAmount += 1;

        Creature? basic = ManifoldCmd.Basic(Owner);
        if (basic is { IsAlive: true })
        {
            await PowerCmd.Apply<CleanWaterPower>(choiceContext, basic, 1, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
