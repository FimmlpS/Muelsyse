using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using Muelsyse.Scripts.Monsters;
using Muelsyse.Scripts.Powers;

namespace Muelsyse.Scripts.Mechanics;

public static class ManifoldCmd
{
    public const int BasicMaxHp = 5;
    public const int BaseThreshold = 1;
    public const int BasicReviveTurns = 2;
    public const int RelicCopyThreshold = 16;
    public const int RelicSourceThreshold = 20;
    public const int MaxRelicCopies = 4;

    public static Creature? Basic(Player player)
    {
        return player.PlayerCombatState?.Pets.FirstOrDefault(pet =>
            pet.Monster is Manifold && pet.HasPower<ManifoldRespawnPower>());
    }

    public static List<Creature> Extras(Player player)
    {
        IReadOnlyList<Creature>? pets = player.PlayerCombatState?.Pets;
        if (pets == null)
        {
            return [];
        }

        return pets.Where(pet =>
            pet.Monster is Manifold && !pet.HasPower<ManifoldRespawnPower>()).ToList();
    }

    /// <summary>All living manifolds, basic first, then extras in spawn order.</summary>
    public static List<Creature> Alive(Player player)
    {
        List<Creature> manifolds = [];
        Creature? basic = Basic(player);
        if (basic is { IsAlive: true })
        {
            manifolds.Add(basic);
        }

        manifolds.AddRange(Extras(player).Where(pet => pet.IsAlive));
        return manifolds;
    }

    public static int AliveHp(Player player)
    {
        return Alive(player).Sum(manifold => manifold.CurrentHp);
    }

    public static int Threshold(Creature manifold)
    {
        int level = manifold.GetPower<ManifoldPower>()?.Amount ?? 0;
        if (level <= 0)
        {
            return 0;
        }

        int multiplier = manifold.GetPower<SourcePower>()?.Amount ?? 1;
        return Math.Max(0, level * multiplier);
    }

    public static int TotalThreshold(Player player)
    {
        return Alive(player).Sum(Threshold);
    }

    public static async Task<ManifoldLinkPower> EnsureLink(PlayerChoiceContext choiceContext, Player player)
    {
        ManifoldLinkPower? link = player.Creature.GetPower<ManifoldLinkPower>();
        if (link != null)
        {
            return link;
        }

        return await PowerCmd.Apply<ManifoldLinkPower>(
            choiceContext, player.Creature, 1m, player.Creature, null)
            ?? throw new InvalidOperationException("Failed to create ManifoldLinkPower.");
    }

    public static async Task<Creature> SpawnBasic(PlayerChoiceContext choiceContext, Player player,
        CardModel? cardSource = null)
    {
        ManifoldLinkPower link = await EnsureLink(choiceContext, player);

        Creature? basic = Basic(player);
        if (basic != null)
        {
            if (!basic.IsAlive)
            {
                await Revive(choiceContext, player, basic, cardSource);
            }

            return basic;
        }

        basic = await PlayerCmd.AddPet<Manifold>(player);

        // AddPet drops the pet next to the player first. Hide it and move it to its real slot right
        // away, otherwise the summon animation plays at the placeholder spot before being teleported.
        HidePet(basic);
        await CreatureCmd.SetMaxHp(basic, BasicMaxHp + link.HpBonus);
        await CreatureCmd.Heal(basic, basic.MaxHp);
        await PowerCmd.Apply<ManifoldRespawnPower>(
            choiceContext, basic, BasicReviveTurns, player.Creature, cardSource);
        await PowerCmd.Apply<ManifoldPower>(
            choiceContext, basic, BaseThreshold + link.ThresholdBonus, player.Creature, cardSource);
        await ApplyPersistentPowers(choiceContext, player, basic, link, cardSource);
        await ApplyCopyObjects(choiceContext, player, basic);
        link.RefreshDisplay();
        ShowPet(basic);
        return basic;
    }

    public static async Task<Creature?> SpawnExtra(PlayerChoiceContext choiceContext, Player player, int turns,
        CardModel? cardSource = null, bool ignoreCap = false)
    {
        if (!ignoreCap && Extras(player).Count(pet => pet.IsAlive) >= MaxRelicCopies)
        {
            return null;
        }

        Creature basic = Basic(player) ?? await SpawnBasic(choiceContext, player, cardSource);
        ManifoldLinkPower link = await EnsureLink(choiceContext, player);

        turns += basic.GetPower<EcologicalCouplingPower>()?.Amount ?? 0;

        Creature extra = await PlayerCmd.AddPet<ManifoldCopy>(player);
        HidePet(extra);
        await CreatureCmd.SetMaxHp(extra, basic.MaxHp);
        await CreatureCmd.Heal(extra, extra.MaxHp);

        int level = basic.GetPower<ManifoldPower>()?.Amount ?? BaseThreshold + link.ThresholdBonus;
        await PowerCmd.Apply<ManifoldPower>(
            choiceContext, extra, level, player.Creature, cardSource);
        await PowerCmd.Apply<ManifoldDisappearPower>(
            choiceContext, extra, turns, player.Creature, cardSource);

        int strength = basic.GetPower<StrengthPower>()?.Amount ?? link.StrengthBonus;
        if (strength != 0)
        {
            await PowerCmd.Apply<StrengthPower>(
                choiceContext, extra, strength, player.Creature, cardSource);
        }

        link.RefreshDisplay();
        ShowPet(extra);
        return extra;
    }

    public static async Task Revive(PlayerChoiceContext choiceContext, Player player, Creature basic,
        CardModel? cardSource = null)
    {
        if (basic.IsAlive)
        {
            return;
        }

        ManifoldLinkPower link = await EnsureLink(choiceContext, player);
        await CreatureCmd.Heal(basic, basic.MaxHp);
        await PowerCmd.Apply<ManifoldPower>(
            choiceContext, basic, BaseThreshold + link.ThresholdBonus, player.Creature, cardSource);
        await ApplyPersistentPowers(choiceContext, player, basic, link, cardSource);
        await ApplyCopyObjects(choiceContext, player, basic);

        ManifoldRespawnPower? respawn = basic.GetPower<ManifoldRespawnPower>();
        respawn?.SetAmount(BasicReviveTurns);
        link.RefreshDisplay();
        ShowPet(basic);
    }

    public static async Task ApplyCopyObjects(PlayerChoiceContext choiceContext, Player player, Creature basic)
    {
        foreach (CardModel card in player.Deck.Cards)
        {
            if (card is IManifoldCopyObject copyObject)
            {
                await copyObject.ApplyToBasicManifold(choiceContext, basic);
            }
        }
    }

    public static async Task TrySteal(PlayerChoiceContext choiceContext, Creature manifold, Creature target)
    {
        CleanWaterPower? cleanWater = manifold.GetPower<CleanWaterPower>();
        if (cleanWater != null)
        {
            await cleanWater.Steal(choiceContext, target);
        }
    }

    private static async Task ApplyPersistentPowers(PlayerChoiceContext choiceContext, Player player,
        Creature basic, ManifoldLinkPower link, CardModel? cardSource)
    {
        if (link.SourceMultiplier > 1)
        {
            await PowerCmd.Apply<SourcePower>(
                choiceContext, basic, link.SourceMultiplier, player.Creature, cardSource);
        }

        if (link.CleanWaterAmount > 0)
        {
            await PowerCmd.Apply<CleanWaterPower>(
                choiceContext, basic, link.CleanWaterAmount, player.Creature, cardSource);
        }

        if (link.StrengthBonus != 0)
        {
            await PowerCmd.Apply<StrengthPower>(
                choiceContext, basic, link.StrengthBonus, player.Creature, cardSource);
        }
    }

    private const float PetGapFromPlayer = 20f;
    private const float PetSpacingFactor = 0.7f;

    /// <summary>
    /// Each extra manifold stands this much higher than the previous one. The health bar and the
    /// power icons are wider than the gap between two sprites, so a flat row makes neighbouring
    /// UI overlap; the staggered heights keep each manifold's bars and powers in their own band.
    /// </summary>
    private const float ExtraPetLift = 40f;

    /// <summary>
    /// Places every manifold in a row to the right of the player, clear of the player's sprite,
    /// re-enables their health bars and makes sure each one shows its attack intent. Extras are
    /// lifted one step each so their UI does not sit on top of the previous manifold's.
    /// </summary>
    public static void ArrangeManifolds(Player player)
    {
        NCombatRoom? room = NCombatRoom.Instance;
        NCreature? playerNode = room?.GetCreatureNode(player.Creature);
        if (room == null || playerNode == null)
        {
            return;
        }

        List<Creature> extras = Extras(player).Where(extra => extra.IsAlive).ToList();

        float x = playerNode.Position.X + playerNode.Visuals.Bounds.Size.X * 0.5f + PetGapFromPlayer;
        float y = playerNode.Position.Y + 10f;

        foreach (Creature manifold in Alive(player))
        {
            NCreature? node = room.GetCreatureNode(manifold);
            if (node == null)
            {
                continue;
            }

            int extraIndex = extras.IndexOf(manifold);
            float lift = extraIndex < 0 ? 0f : ExtraPetLift * (extraIndex + 1);

            float width = node.Visuals.Bounds.Size.X;
            node.Position = new Vector2(x + width * 0.5f, y - lift);
            x += width * PetSpacingFactor;

            node.ToggleIsInteractable(true);
            ManifoldIntentDisplay.Attach(manifold);
        }
    }

    /// <summary>
    /// Hides a freshly added pet and moves it straight to its final slot, before the summon
    /// animation has a chance to play at the placeholder position next to the player.
    /// </summary>
    private static void HidePet(Creature pet)
    {
        if (pet.PetOwner is { } player)
        {
            ArrangeManifolds(player);
        }

        if (NCombatRoom.Instance?.GetCreatureNode(pet) is { } node)
        {
            node.Modulate = Colors.Transparent;
        }
    }

    private static void ShowPet(Creature pet)
    {
        if (pet.PetOwner is { } player)
        {
            ArrangeManifolds(player);
        }

        NCreature? node = NCombatRoom.Instance?.GetCreatureNode(pet);
        if (node == null)
        {
            return;
        }

        node.Modulate = Colors.Transparent;

        // Restart the summon animation now that the pet stands in its real slot, so the whole
        // "Start" clip plays in place instead of finishing at the placeholder and snapping over.
        node.SetAnimationTrigger("Revive");

        Tween tween = node.CreateTween();
        tween.TweenProperty(node, "modulate", Colors.White, 0.35).SetDelay(0.05);
    }
}
