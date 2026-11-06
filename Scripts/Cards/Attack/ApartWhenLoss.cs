using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Cards.Status;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Mechanics.Stroll;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Attack;

/// <summary>
/// 在水散时相别: a 协同 strike that grows with every unit of 水 exhausted this combat.
/// </summary>
[Pool(typeof(MuelsyseCardPool))]
public class ApartWhenLoss : AbstractMuelsyseCard, IOnStrolled
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculationBaseVar(5m),
        new ExtraDamageVar(1m),
        new TogetherCalculatedDamageVar(ValueProp.Move).WithMultiplier(ExhaustedWater)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [MuelsyseKeywords.Together];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Together),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Stroll),
        .. HoverTipFactory.FromCardWithCardHoverTips<Water>()
    ];

    public ApartWhenLoss() : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    private static decimal ExhaustedWater(CardModel card, Creature? target)
    {
        return WaterLedger.Exhausted;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    public async Task OnStrolled(PlayerChoiceContext choiceContext, Player player, bool first)
    {
        if (player != Owner)
        {
            return;
        }

        await StrollCmd.PlayCopy(choiceContext, this);
    }

    protected override void OnUpgrade() => DynamicVars.ExtraDamage.UpgradeValueBy(1);
}
