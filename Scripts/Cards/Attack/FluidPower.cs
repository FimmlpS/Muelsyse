using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Mechanics.Stroll;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Powers;

namespace Muelsyse.Scripts.Cards.Attack;

/// <summary>流体力学: temporary strength and a 协同 strike, replayed when 漫步 chooses it.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class FluidPower : AbstractMuelsyseCard, IOnStrolled
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new TogetherDamageVar(3, ValueProp.Move),
        new DynamicVar("Strength", 1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [MuelsyseKeywords.Together];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Together),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Stroll)
    ];

    public FluidPower() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        await PowerCmd.Apply<FluidStrengthPower>(choiceContext, Owner.Creature,
            DynamicVars["Strength"].IntValue, Owner.Creature, this);
    }

    public async Task OnStrolled(PlayerChoiceContext choiceContext, Player player, bool first)
    {
        if (player != Owner)
        {
            return;
        }

        await StrollCmd.PlayCopy(choiceContext, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["Strength"].UpgradeValueBy(1);
    }
}
