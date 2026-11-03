using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Relics;

namespace Muelsyse.Scripts.Cards.Skill;

[Pool(typeof(MuelsyseCardPool))]
public class WaterRecycle : AbstractMuelsyseCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Water", 1)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water)
    ];

    public WaterRecycle() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        HashSet<CardType> types = [];
        for (int i = 0; i < 20; i++)
        {
            List<CardModel> drawn = (await CardPileCmd.Draw(choiceContext, 1, Owner)).ToList();
            if (drawn.Count == 0)
            {
                break;
            }

            await LifeCircle.AddWater(choiceContext, Owner, DynamicVars["Water"].IntValue, this);

            foreach (CardModel card in drawn.Where(card => card.Type != CardType.Status))
            {
                types.Add(card.Type);
            }

            if (types.Count >= 2)
            {
                break;
            }
        }
    }

    protected override void OnUpgrade() => DynamicVars["Water"].UpgradeValueBy(1);
}
