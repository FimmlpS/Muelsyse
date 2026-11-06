using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Powers;

namespace Muelsyse.Scripts.Cards.Power;

/// <summary>遗尘: every 漫步 also shows a free random card.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class RemainDust : AbstractMuelsyseCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Stroll)
    ];

    public RemainDust() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<DustPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
