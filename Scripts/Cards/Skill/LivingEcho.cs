using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Cards.Status;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>生灵的回响: exhausts hand cards and turns each of them into water.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class LivingEcho : AbstractMuelsyseCard
{
    private static readonly LocString SelectPrompt = new("card_selection", "LIVING_ECHO_SELECT");

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Echo", 3)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water),
        .. HoverTipFactory.FromCardWithCardHoverTips<Water>()
    ];

    public LivingEcho() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int limit = DynamicVars["Echo"].IntValue;
        if (limit <= 0)
        {
            return;
        }

        IEnumerable<CardModel> picked = await CardSelectCmd.FromHand(choiceContext, Owner,
            new CardSelectorPrefs(SelectPrompt, 0, limit), null, this);

        int count = 0;
        foreach (CardModel card in picked.ToList())
        {
            await CardCmd.Exhaust(choiceContext, card);
            count++;
        }

        for (int i = 0; i < count; i++)
        {
            await WaterCmd.AddToHand(choiceContext, Owner);
        }
    }

    protected override void OnUpgrade() => DynamicVars["Echo"].UpgradeValueBy(2);
}
