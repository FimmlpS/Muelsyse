using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Enums;
using Muelsyse.Scripts.Mechanics;
using Muelsyse.Scripts.Mechanics.Cultivate;
using Muelsyse.Scripts.Powers;

namespace Muelsyse.Scripts.Cards.Status;

/// <summary>
/// 水: an unplayable status. Whenever it enters a non-hand pile (draw, discard or exhaust) it splashes a
/// random enemy and plants one random cultivation effect on itself, so it can be used as a richer
/// material later.
/// </summary>
[Pool(typeof(TokenCardPool))]
public class Water : AbstractMuelsyseCard, ICultivateExhaust, ICultivateUnlimited
{
    /// <summary>Attack / Block / 润化 + the four pollen, matching S1's random cultivation pool.</summary>
    private const int AimCount = 7;

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new WaterSplashVar(3)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Unplayable,
        MuelsyseKeywords.ExhaustMaterial
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [
        HoverTipFactory.FromKeyword(MuelsyseKeywords.ExhaustMaterial),
        HoverTipFactory.FromKeyword(MuelsyseKeywords.Cultivate)
    ];

    public Water() : base(-1, CardType.Status, CardRarity.Token, TargetType.None)
    {
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType,
        AbstractModel? clonedBy)
    {
        if (card != this || Owner == null || CombatState == null)
        {
            return;
        }

        PileType newPile = card.Pile?.Type ?? PileType.None;
        if (newPile == oldPileType
            || newPile is not (PileType.Draw or PileType.Discard or PileType.Exhaust)
            || !CombatState.IsLiveCombat())
        {
            return;
        }

        if (newPile == PileType.Exhaust)
        {
            WaterLedger.RegisterExhausted();
        }

        await Splash();
        CultivateCmd.AddBonus(this, RandomAim(Owner), 1);
    }

    private async Task Splash()
    {
        if (Owner == null || CombatState is not { } combatState)
        {
            return;
        }

        List<Creature> enemies = combatState.HittableEnemies
            .Where(enemy => enemy.IsAlive && enemy.CombatState?.IsLiveCombat() == true)
            .ToList();
        if (enemies.Count == 0)
        {
            return;
        }

        Creature target = Owner.RunState.Rng.CombatCardSelection.NextItem(enemies);
        decimal amount = DynamicVars[WaterSplashVar.Key].BaseValue
                         + (Owner.Creature.GetPower<FlowerWaterPower>()?.Amount ?? 0);

        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), target, amount,
            ValueProp.Unpowered, Owner.Creature, this, null);
    }

    private static CultivateAim RandomAim(Player player)
    {
        return player.RunState.Rng.CombatCardSelection.NextInt(AimCount) switch
        {
            0 => CultivateAim.Attack,
            1 => CultivateAim.Block,
            2 => CultivateAim.Water,
            3 => CultivateAim.Spread,
            4 => CultivateAim.Parasitism,
            5 => CultivateAim.Cure,
            _ => CultivateAim.WaterGrow
        };
    }

    protected override void OnUpgrade()
    {
        DynamicVars[WaterSplashVar.Key].UpgradeValueBy(1);
    }
}
