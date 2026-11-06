using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using Muelsyse.Scripts.Cards.Status;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>矩阵浪潮: every living manifold adds one unit of water; Muelsyse no longer adds one herself.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class MatrixWave : AbstractMuelsyseCard
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Manifold),
        .. HoverTipFactory.FromCardWithCardHoverTips<Water>(IsUpgraded)
    ];

    public MatrixWave() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // One unit per manifold, nothing extra for Muelsyse herself: the card no longer pays a
        // base amount on top of the multiplier.
        int manifolds = ManifoldCmd.Alive(Owner).Count;
        for (int i = 0; i < manifolds; i++)
        {
            await WaterCmd.AddToHand(choiceContext, Owner, IsUpgraded);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
