using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Cards.Status;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Attack;

/// <summary>水连击: one hit for each of up to four 水, taken from draw, then hand, then discard.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class WaterWave : AbstractMuelsyseCard
{
    private const int MaxWater = 4;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(6, ValueProp.Move)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water),
        .. HoverTipFactory.FromCardWithCardHoverTips<Water>()
    ];

    public WaterWave() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target is not { } target)
        {
            return;
        }

        List<CardModel> waters = WaterCmd.InConsumptionOrder(Owner, MaxWater);

        foreach (CardModel _ in waters)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(target)
                .Execute(choiceContext);
        }

        foreach (CardModel water in waters)
        {
            await CardCmd.Exhaust(choiceContext, water);
        }
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}
