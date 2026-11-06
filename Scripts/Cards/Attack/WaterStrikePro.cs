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
public class WaterStrikePro : AbstractMuelsyseCard
{
    public const string FirstWordKey = "FirstWord";

    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new TogetherDamageVar(6, ValueProp.Move),
        new DynamicVar("Water", 2),
        new DynamicVar(FirstWordKey, 1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [MuelsyseKeywords.Together];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Together),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water)
    ];

    public WaterStrikePro() : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override Task BeforeCombatStart()
    {
        DynamicVars[FirstWordKey].BaseValue = 1;
        return Task.CompletedTask;
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        for (int i = 0; i < 2; i++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .Execute(choiceContext);
        }

        if (DynamicVars[FirstWordKey].IntValue > 0)
        {
            DynamicVars[FirstWordKey].BaseValue -= 1;
            await LifeCircle.AddWater(choiceContext, Owner, DynamicVars["Water"].IntValue, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
        DynamicVars["Water"].UpgradeValueBy(1);
    }
}
