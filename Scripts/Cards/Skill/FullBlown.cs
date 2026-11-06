using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>繁花: plants a burst of pollen on a card in hand.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class FullBlown : AbstractMuelsyseCard
{
    private static readonly LocString SelectPrompt = new("card_selection", "POLLEN_SELECT");

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Pollen", 2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Pollen)
    ];

    public FullBlown() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel? target = (await CardSelectCmd.FromHand(choiceContext, Owner,
            new CardSelectorPrefs(SelectPrompt, 1), null, this)).FirstOrDefault();
        if (target == null)
        {
            return;
        }

        PollenCmd.PlantRandom(target, DynamicVars["Pollen"].IntValue);
        CultivateCmd.RefreshVisuals(target);
    }

    protected override void OnUpgrade() => DynamicVars["Pollen"].UpgradeValueBy(2);
}
