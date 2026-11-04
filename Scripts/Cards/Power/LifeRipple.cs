using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Powers;

namespace Muelsyse.Scripts.Cards.Power;

/// <summary>生命涟漪: every manifold shares a ripple of block at turn's end.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class LifeRipple : AbstractMuelsyseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("BlockPerManifold", 2)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Manifold)
    ];

    public LifeRipple() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<RipplePower>(choiceContext, Owner.Creature,
            DynamicVars["BlockPerManifold"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["BlockPerManifold"].UpgradeValueBy(1);
}
