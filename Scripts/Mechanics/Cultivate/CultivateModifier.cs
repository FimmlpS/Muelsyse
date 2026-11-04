using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Muelsyse.Scripts.Powers;
using Muelsyse.Scripts.Relics;

namespace Muelsyse.Scripts.Mechanics.Cultivate;

/// <summary>
/// All cultivated values ever applied to a card. The totals survive saving and cloning,
/// and are what the card's icon row and the 失肥/play effects read from.
/// Attack is applied through <see cref="ModifyBaseDamageAdditive"/> so it flows through
/// the ordinary damage preview; block and cost are baked into the card when applied.
/// </summary>
public class CultivateModifier : CardModifier
{
    private Dictionary<string, int> _values = new();

    public int Attack => Get(CultivateAim.Attack);
    public int Block => Get(CultivateAim.Block);
    public int Cost => Get(CultivateAim.Cost);
    public int Draw => Get(CultivateAim.Draw);
    public int Energy => Get(CultivateAim.Energy);
    public int Water => Get(CultivateAim.Water);
    public int Spread => Get(CultivateAim.Spread);
    public int Parasitism => Get(CultivateAim.Parasitism);
    public int Cure => Get(CultivateAim.Cure);
    public int WaterGrow => Get(CultivateAim.WaterGrow);

    public int Get(CultivateAim aim) => _values.GetValueOrDefault(Key(aim));

    public void Add(CultivateAim aim, int amount)
    {
        if (amount == 0)
        {
            return;
        }

        _values[Key(aim)] = Get(aim) + amount;
    }

    public void Add(CultivateBonus bonus)
    {
        Add(CultivateAim.Attack, bonus.AttackDelta);
        Add(CultivateAim.Block, bonus.BlockDelta);
        Add(CultivateAim.Cost, bonus.CostDelta);
        Add(CultivateAim.Draw, bonus.DrawAmount);
        Add(CultivateAim.Energy, bonus.EnergyAmount);
        Add(CultivateAim.Water, bonus.WaterAmount);
        Add(CultivateAim.Spread, bonus.SpreadAmount);
        Add(CultivateAim.Parasitism, bonus.ParasitismAmount);
        Add(CultivateAim.Cure, bonus.CureAmount);
        Add(CultivateAim.WaterGrow, bonus.WaterGrowAmount);
    }

    public bool HasAny => _values.Values.Any(value => value != 0);

    /// <summary>Applies the block/cost part of a bonus to one concrete card instance.</summary>
    public static void BakePermanentStats(CardModel card, int blockDelta, int costDelta)
    {
        if (blockDelta != 0 && card.DynamicVars.TryGetValue("Block", out DynamicVar? block))
        {
            block.BaseValue += blockDelta;
        }

        if (costDelta != 0 && !card.EnergyCost.CostsX)
        {
            int current = card.EnergyCost.GetWithModifiers(CostModifiers.None);
            card.EnergyCost.SetCustomBaseCost(Math.Max(0, current + costDelta));
        }
    }

    public override decimal ModifyBaseDamageAdditive(decimal originalDamage, ValueProp props)
    {
        return Attack;
    }

    public override void OnDowngrade()
    {
        base.OnDowngrade();
        if (Owner != null)
        {
            BakePermanentStats(Owner, Block, Cost);
        }
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner?.Owner is not { } player)
        {
            return;
        }

        int draw = Draw;
        if (draw > 0)
        {
            await CardPileCmd.Draw(choiceContext, draw, player);
        }

        int energy = Energy;
        if (energy > 0)
        {
            await PlayerCmd.GainEnergy(energy, player);
        }

        int water = Water;
        if (water > 0)
        {
            await LifeCircle.AddWater(choiceContext, player, water, Owner);
        }

        Creature? enemy = cardPlay.Target
                          ?? player.Creature.CombatState?.HittableEnemies.FirstOrDefault();

        if (Spread > 0 && enemy != null)
        {
            await PowerCmd.Apply<FlowerSpreadPower>(
                choiceContext, enemy, Spread, player.Creature, Owner);
        }

        if (Parasitism > 0 && enemy != null)
        {
            await PowerCmd.Apply<FlowerParasitismPower>(
                choiceContext, enemy, Parasitism, player.Creature, Owner);
        }

        if (Cure > 0)
        {
            await PowerCmd.Apply<FlowerCurePower>(
                choiceContext, player.Creature, Cure, player.Creature, Owner);
        }

        if (WaterGrow > 0)
        {
            await PowerCmd.Apply<FlowerWaterPower>(
                choiceContext, player.Creature, WaterGrow, player.Creature, Owner);
        }
    }

    public override void StoreSaveData(ModifierSave save)
    {
        foreach ((string key, int value) in _values)
        {
            save.IntProperties[key] = value;
        }
    }

    public override void LoadSaveData(ModifierSave save)
    {
        _values = new Dictionary<string, int>(save.IntProperties);
    }

    protected override void DeepCloneFields()
    {
        base.DeepCloneFields();
        _values = new Dictionary<string, int>(_values);
    }

    private static string Key(CultivateAim aim) => aim.ToString();
}
