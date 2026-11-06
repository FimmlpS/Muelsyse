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
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Pools;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>
/// 模式化培育: fetches a cultivation card back from the draw or discard pile, then adds water.
/// </summary>
[Pool(typeof(MuelsyseCardPool))]
public class ModeCultivate : AbstractMuelsyseCard
{
    private static readonly LocString SelectPrompt = new("card_selection", "MODE_CULTIVATE_SELECT");

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Water", 1)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Cultivate),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water),
        .. HoverTipFactory.FromCardWithCardHoverTips<Water>()
    ];

    public ModeCultivate() : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await FetchCultivationCard(choiceContext);

        for (int i = 0; i < DynamicVars["Water"].IntValue; i++)
        {
            await WaterCmd.AddToHand(choiceContext, Owner);
        }
    }

    private async Task FetchCultivationCard(PlayerChoiceContext choiceContext)
    {
        List<CardModel> candidates = [];
        candidates.AddRange(PileType.Draw.GetPile(Owner).Cards);
        candidates.AddRange(PileType.Discard.GetPile(Owner).Cards);
        candidates.RemoveAll(card => card is not ICultivateCard);

        if (candidates.Count == 0)
        {
            return;
        }

        if (candidates.Count == 1)
        {
            await CardPileCmd.Add(candidates[0], PileType.Hand);
            return;
        }

        IEnumerable<CardModel> picked = await CardSelectCmd.FromSimpleGrid(
            choiceContext, candidates, Owner, new CardSelectorPrefs(SelectPrompt, 1));

        foreach (CardModel card in picked)
        {
            await CardPileCmd.Add(card, PileType.Hand);
        }
    }

    protected override void OnUpgrade() => DynamicVars["Water"].UpgradeValueBy(1);
}
