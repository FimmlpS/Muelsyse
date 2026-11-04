using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Pools;
using Muelsyse.Scripts.Relics;

namespace Muelsyse.Scripts.Cards.Skill;

/// <summary>不竭之泉: an inexhaustible water source that never dries out.</summary>
[Pool(typeof(MuelsyseCardPool))]
public class EndlessSpring : AbstractMuelsyseCard, IWontDry, IOnCultivated
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Water", 2)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Water)
    ];

    public EndlessSpring() : base(-1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    public async Task OnCultivated(PlayerChoiceContext choiceContext, Player player, bool isMain,
        CardModel? target)
    {
        await LifeCircle.AddWater(choiceContext, player, DynamicVars["Water"].IntValue, this);
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType,
        AbstractModel? clonedBy)
    {
        if (card != this || Pile?.Type != PileType.Draw || oldPileType == PileType.Draw
            || CombatState == null || Owner.PlayerCombatState == null)
        {
            return;
        }

        await SpringWater();
    }

    /// <summary>
    /// The draw pile is also filled at combat start, but the engine does that by moving the freshly
    /// cloned deck cards in directly (<c>Player.PopulateCombatState</c>), which never dispatches
    /// <see cref="AfterCardChangedPiles"/>. This hook runs right after that population, while the
    /// card still sits in the draw pile.
    /// </summary>
    public override async Task BeforeCombatStart()
    {
        if (Pile?.Type != PileType.Draw || CombatState == null || Owner.PlayerCombatState == null)
        {
            return;
        }

        await SpringWater();
    }

    private async Task SpringWater()
    {
        await LifeCircle.AddWater(new BlockingPlayerChoiceContext(), Owner,
            DynamicVars["Water"].IntValue, this);
    }

    protected override void OnUpgrade() => DynamicVars["Water"].UpgradeValueBy(1);
}
