using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Powers;

namespace Muelsyse.Scripts.Cards.Power;

/// <summary>塑形：生态耦合: manifolds made from now on stay longer.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class EcologicalCoupling : AbstractMuelsyseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Turns", 1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Molding),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Disappear)
    ];

    public EcologicalCoupling() : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int turns = DynamicVars["Turns"].IntValue;

        if (ManifoldCmd.Basic(Owner) is not { } basic)
        {
            return;
        }

        await PowerCmd.Apply<EcologicalCouplingPower>(choiceContext, basic,
            turns, Owner.Creature, this);

        foreach (Creature manifold in ManifoldCmd.Extras(Owner))
        {
            if (!manifold.IsAlive)
            {
                continue;
            }

            await PowerCmd.Apply<ManifoldDisappearPower>(choiceContext, manifold,
                turns, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade() => DynamicVars["Turns"].UpgradeValueBy(1);
}
