using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>浅层非熵适应: every card in hand learns to water itself when played.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class ShallowAdaption : AbstractMuelsyseCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Water", 1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water)
    ];

    public ShallowAdaption() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.PlayerCombatState is not { } combatState)
        {
            return Task.CompletedTask;
        }

        int water = DynamicVars["Water"].IntValue;
        foreach (CardModel card in combatState.Hand.Cards.ToList())
        {
            CultivateCmd.AddBonus(card, CultivateAim.Water, water);
        }

        return Task.CompletedTask;
    }

    protected override void OnUpgrade() => DynamicVars["Water"].UpgradeValueBy(1);
}
