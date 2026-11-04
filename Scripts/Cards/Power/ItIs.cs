using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Powers;

namespace Muelsyse.Scripts.Cards.Power;

/// <summary>祂在: for a precious few rounds, blows glance off.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class ItIs : AbstractMuelsyseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Turns", 1)
    ];

    public ItIs() : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ItIsPower>(choiceContext, Owner.Creature,
            DynamicVars["Turns"].IntValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Turns"].UpgradeValueBy(1);
}
