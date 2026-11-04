using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>回流: recycles every dried-out card into fresh draws and energy.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class Reflux : AbstractMuelsyseCard, ICultivateNutrient
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("NutrientEnergy", 1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Nutrient),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Dry)
    ];

    public IEnumerable<CultivateCalculator> NutrientCalculators => [
        CultivateCalculator.Add(CultivateAim.Energy, DynamicVars["NutrientEnergy"].IntValue)
    ];

    public Reflux() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.PlayerCombatState is not { } combatState)
        {
            return;
        }

        List<CardModel> dry = combatState.Hand.Cards
            .Where(CultivateDryModifier.IsDry)
            .ToList();

        if (dry.Count > 0)
        {
            await CardCmd.DiscardAndDraw(choiceContext, dry, dry.Count);
        }
    }

    protected override void OnUpgrade() => DynamicVars["NutrientEnergy"].UpgradeValueBy(1);
}
