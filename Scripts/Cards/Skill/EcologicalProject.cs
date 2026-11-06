using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Powers;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>生态科课题: 水培 stacks, doubled when this card is 失肥.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class EcologicalProject : AbstractMuelsyseCard
{
    private const int DryBonus = 2;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("WaterGrow", 2)
    ];

    public EcologicalProject() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int amount = DynamicVars["WaterGrow"].IntValue
                     + (CultivateDryModifier.IsDry(this) ? DryBonus : 0);

        await PowerCmd.Apply<FlowerWaterPower>(choiceContext, Owner.Creature, amount,
            Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["WaterGrow"].UpgradeValueBy(2);
}
