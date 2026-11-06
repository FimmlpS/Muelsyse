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

namespace Muelsyse.Scripts.Cards.Attack;

/// <summary>
/// 漫步于黄金之梦: starts at 0 damage. Every time the draw pile is shuffled, a copy sitting in the
/// draw pile slips into the discard pile and gains damage for the rest of the combat (the growth is
/// combat-only; the card resets between fights). The shuffle rule itself is
/// <see cref="Mechanics.GoldenWanderListener"/>, which scans the draw pile after every shuffle.
/// </summary>
[Pool(typeof(MuelsyseCardPool))]
public class GoldenWander : AbstractMuelsyseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new TogetherDamageVar(0, ValueProp.Move),
        new DynamicVar("Growth", 4)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [MuelsyseKeywords.Together];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Together),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Stroll)
    ];

    public GoldenWander() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    /// <summary>Called by <see cref="Mechanics.GoldenWanderListener"/> once per shuffle it survives.</summary>
    public void GrowOnShuffle()
    {
        DynamicVars.Damage.BaseValue += DynamicVars["Growth"].IntValue;
    }

    protected override void OnUpgrade() => DynamicVars["Growth"].UpgradeValueBy(2);
}
