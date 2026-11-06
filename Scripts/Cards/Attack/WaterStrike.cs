using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Relics;

namespace Muelsyse.Scripts.Cards.Attack;

[Pool(typeof(MuelsyseCardPool))]
public class WaterStrike : AbstractMuelsyseCard
{
    public const string FirstWordKey = "FirstWord";

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new TogetherDamageVar(6, ValueProp.Move),
        new DynamicVar(FirstWordKey, 0)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [MuelsyseKeywords.Together];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Together),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water)
    ];

    public WaterStrike() : base(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
    {
    }

    public override Task BeforeCombatStart()
    {
        DynamicVars[FirstWordKey].BaseValue = IsUpgraded ? 1 : 0;
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        if (DynamicVars[FirstWordKey].IntValue > 0)
        {
            DynamicVars[FirstWordKey].BaseValue -= 1;
            await LifeCircle.AddWater(choiceContext, Owner, 1, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars[FirstWordKey].BaseValue = 1;
    }
}
