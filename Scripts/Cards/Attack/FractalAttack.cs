using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Cards.Status;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Attack;

/// <summary>分形夹击: a 协同 strike that leaves water behind in the discard pile.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class FractalAttack : AbstractMuelsyseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new TogetherDamageVar(4, ValueProp.Move),
        new DynamicVar("Water", 2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [MuelsyseKeywords.Together];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Together),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water),
        .. HoverTipFactory.FromCardWithCardHoverTips<Water>(IsUpgraded)
    ];

    public FractalAttack() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        for (int i = 0; i < DynamicVars["Water"].IntValue; i++)
        {
            await WaterCmd.AddToDiscard(choiceContext, Owner, IsUpgraded);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}
