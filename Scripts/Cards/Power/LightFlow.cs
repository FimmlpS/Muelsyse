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
public class LightFlow : AbstractMuelsyseCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [MuelsyseKeywords.Molding];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Molding),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Threshold)
    ];

    public LightFlow() : base(2, CardType.Power, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ManifoldLinkPower link = await ManifoldCmd.EnsureLink(choiceContext, Owner);
        link.ThresholdBonus += 1;
        link.HpBonus += 5;

        foreach (Creature manifold in ManifoldCmd.Alive(Owner))
        {
            await CreatureCmd.GainMaxHp(manifold, 5);
            await PowerCmd.Apply<ManifoldPower>(choiceContext, manifold, 1, Owner.Creature, this);
        }

        link.RefreshDisplay();
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
